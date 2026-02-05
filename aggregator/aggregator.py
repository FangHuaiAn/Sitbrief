"""
Sitbrief Aggregator - 智庫/媒體首頁標題聚合器
支援網頁抓取和 RSS Feed 兩種模式
"""

import asyncio
import json
import re
import xml.etree.ElementTree as ET
from datetime import datetime, timezone
from pathlib import Path
from urllib.parse import urljoin, urlparse

import httpx
import yaml
from playwright.async_api import async_playwright


class HeadlineAggregator:
    def __init__(self, config_path: str = "sources.yaml"):
        self.config_path = Path(config_path)
        self.sources = self._load_sources()
        self.output_dir = Path(__file__).parent / "output"
        self.output_dir.mkdir(exist_ok=True)

    def _load_sources(self) -> list:
        with open(self.config_path, "r", encoding="utf-8") as f:
            config = yaml.safe_load(f)
        return config.get("sources", [])

    async def fetch_rss(self, source: dict) -> list:
        """從 RSS Feed 抓取標題"""
        headlines = []
        name = source["name"]
        feeds = source.get("feeds", [])
        
        print(f"📡 抓取 {name} (RSS)...")
        
        async with httpx.AsyncClient() as client:
            for feed_url in feeds:
                try:
                    response = await client.get(feed_url, timeout=30)
                    response.raise_for_status()
                    
                    root = ET.fromstring(response.text)
                    
                    # 處理 Atom feed
                    ns = {'atom': 'http://www.w3.org/2005/Atom'}
                    entries = root.findall('.//atom:entry', ns)
                    
                    for entry in entries[:20]:  # 每個 feed 最多 20 則
                        title_elem = entry.find('atom:title', ns)
                        link_elem = entry.find('atom:link[@rel="alternate"]', ns)
                        
                        if title_elem is not None and link_elem is not None:
                            title = title_elem.text.strip() if title_elem.text else ""
                            url = link_elem.get('href', '')
                            
                            if title and url:
                                headlines.append({
                                    "title": title,
                                    "url": url,
                                    "source": name
                                })
                                
                except Exception as e:
                    print(f"  ⚠️ RSS 抓取失敗 {feed_url}: {e}")
        
        # 去重
        seen = set()
        unique = []
        for h in headlines:
            if h['url'] not in seen:
                seen.add(h['url'])
                unique.append(h)
        
        print(f"  ✅ 取得 {len(unique)} 則標題")
        return unique

    async def fetch_web(self, source: dict) -> list:
        """抓取單一來源的標題"""
        headlines = []
        name = source["name"]
        url = source["url"]
        selectors = source.get("selectors", {})
        exclude_patterns = selectors.get("exclude", [])

        print(f"📡 抓取 {name}...")

        async with async_playwright() as p:
            browser = await p.chromium.launch(headless=True)
            page = await browser.new_page()
            
            try:
                await page.goto(url, wait_until="networkidle", timeout=30000)
                
                # 取得所有文章連結 - 支援多個選擇器
                article_selector = selectors.get("articles", "a")
                
                # 如果選擇器包含逗號，分別處理
                all_links = []
                for selector in article_selector.split(","):
                    selector = selector.strip()
                    try:
                        links = await page.query_selector_all(selector)
                        all_links.extend(links)
                    except Exception:
                        pass
                
                seen_urls = set()
                
                for link in all_links:
                    try:
                        href = await link.get_attribute("href")
                        text = await link.inner_text()
                        text = text.strip()
                        
                        if not href or not text:
                            continue
                        
                        # 轉換相對路徑
                        full_url = urljoin(url, href)
                        
                        # 檢查排除模式
                        if any(pattern in full_url for pattern in exclude_patterns):
                            continue
                        
                        # 去重
                        if full_url in seen_urls:
                            continue
                        seen_urls.add(full_url)
                        
                        # 清理標題（移除多餘空白）
                        text = re.sub(r'\s+', ' ', text).strip()
                        
                        # 過濾太短的標題
                        if len(text) < 10:
                            continue
                        
                        headlines.append({
                            "title": text,
                            "url": full_url,
                            "source": name
                        })
                        
                    except Exception as e:
                        continue
                
            except Exception as e:
                print(f"  ❌ 抓取失敗: {e}")
            finally:
                await browser.close()
        
        print(f"  ✅ 取得 {len(headlines)} 則標題")
        return headlines

    async def fetch_source(self, source: dict) -> list:
        """根據類型選擇抓取方式"""
        source_type = source.get("type", "web")
        if source_type == "rss":
            return await self.fetch_rss(source)
        else:
            return await self.fetch_web(source)

    async def fetch_all(self) -> dict:
        """抓取所有來源"""
        all_headlines = []
        
        for source in self.sources:
            headlines = await self.fetch_source(source)
            all_headlines.extend(headlines)
        
        result = {
            "fetchedAt": datetime.now(timezone.utc).isoformat(),
            "totalCount": len(all_headlines),
            "sources": [s["name"] for s in self.sources],
            "headlines": all_headlines
        }
        
        return result

    def save_result(self, result: dict, filename: str = "headlines.json"):
        """儲存結果"""
        output_path = self.output_dir / filename
        with open(output_path, "w", encoding="utf-8") as f:
            json.dump(result, f, ensure_ascii=False, indent=2)
        print(f"\n📁 已儲存到 {output_path}")
        return output_path

    def save_html(self, result: dict, filename: str = "headlines.html"):
        """儲存 HTML 格式結果"""
        output_path = self.output_dir / filename
        
        # 按來源分組
        by_source = {}
        for h in result["headlines"]:
            source = h["source"]
            if source not in by_source:
                by_source[source] = []
            by_source[source].append(h)
        
        # 格式化時間
        fetched_at = result["fetchedAt"][:19].replace("T", " ")
        
        html = f'''<!DOCTYPE html>
<html lang="zh-TW">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sitbrief Headlines - {fetched_at}</title>
    <style>
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background: #f5f5f7;
            color: #1d1d1f;
            line-height: 1.5;
            padding: 20px;
        }}
        .container {{ max-width: 900px; margin: 0 auto; }}
        header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 12px;
            margin-bottom: 24px;
        }}
        header h1 {{ font-size: 28px; margin-bottom: 8px; }}
        header .meta {{ opacity: 0.9; font-size: 14px; }}
        .source-section {{
            background: white;
            border-radius: 12px;
            padding: 24px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.06);
        }}
        .source-section h2 {{
            font-size: 18px;
            color: #667eea;
            margin-bottom: 16px;
            padding-bottom: 8px;
            border-bottom: 2px solid #f0f0f0;
        }}
        .source-section h2 span {{
            background: #667eea;
            color: white;
            font-size: 12px;
            padding: 2px 8px;
            border-radius: 12px;
            margin-left: 8px;
        }}
        ul {{ list-style: none; }}
        li {{ 
            padding: 12px 0;
            border-bottom: 1px solid #f0f0f0;
        }}
        li:last-child {{ border-bottom: none; }}
        a {{ 
            color: #1d1d1f;
            text-decoration: none;
            display: block;
        }}
        a:hover {{ color: #667eea; }}
        .url {{
            font-size: 12px;
            color: #86868b;
            margin-top: 4px;
            word-break: break-all;
        }}
        footer {{
            text-align: center;
            padding: 20px;
            color: #86868b;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class="container">
        <header>
            <h1>📰 Sitbrief Headlines</h1>
            <div class="meta">
                更新時間：{fetched_at} UTC<br>
                來源：{', '.join(result['sources'])} | 共 {result['totalCount']} 則
            </div>
        </header>
'''
        
        for source, headlines in by_source.items():
            html += f'''
        <section class="source-section">
            <h2>{source} <span>{len(headlines)}</span></h2>
            <ul>
'''
            for h in headlines:
                title = h["title"].replace("<", "&lt;").replace(">", "&gt;")
                url = h["url"]
                domain = urlparse(url).netloc
                html += f'''                <li>
                    <a href="{url}" target="_blank">{title}</a>
                    <div class="url">{domain}</div>
                </li>
'''
            html += '''            </ul>
        </section>
'''
        
        html += '''
        <footer>
            Generated by Sitbrief Aggregator
        </footer>
    </div>
</body>
</html>
'''
        
        with open(output_path, "w", encoding="utf-8") as f:
            f.write(html)
        print(f"📄 已儲存 HTML 到 {output_path}")
        return output_path

    def print_summary(self, result: dict, limit: int = 10):
        """印出摘要"""
        print(f"\n{'='*60}")
        print(f"📰 聚合結果摘要")
        print(f"{'='*60}")
        print(f"抓取時間: {result['fetchedAt']}")
        print(f"來源數量: {len(result['sources'])}")
        print(f"總標題數: {result['totalCount']}")
        print(f"\n最新 {limit} 則標題：")
        print("-" * 60)
        
        for i, h in enumerate(result["headlines"][:limit], 1):
            title = h["title"][:50] + "..." if len(h["title"]) > 50 else h["title"]
            print(f"{i:2}. [{h['source']}] {title}")


async def main():
    aggregator = HeadlineAggregator()
    result = await aggregator.fetch_all()
    aggregator.save_result(result)
    aggregator.save_html(result)
    aggregator.print_summary(result)


if __name__ == "__main__":
    asyncio.run(main())
