import asyncio
from playwright.async_api import async_playwright

async def debug():
    async with async_playwright() as p:
        browser = await p.chromium.launch(
            headless=True,
            args=['--disable-blink-features=AutomationControlled']
        )
        context = await browser.new_context(
            user_agent='Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36',
            viewport={'width': 1920, 'height': 1080}
        )
        page = await context.new_page()
        
        await page.goto('https://www.rand.org', wait_until='domcontentloaded', timeout=60000)
        await page.wait_for_timeout(5000)
        
        links = await page.query_selector_all('a')
        print(f'Total links: {len(links)}')
        
        count = 0
        for link in links:
            href = await link.get_attribute('href')
            if href and '/pubs/' in href:
                text = (await link.inner_text()).strip()[:60]
                if text and len(text) > 10:
                    print(f'{text}')
                    print(f'  -> {href}')
                    count += 1
                    if count >= 10:
                        break
        
        if count == 0:
            print('No pubs links found. Checking page content...')
            html = await page.content()
            print(f'Page length: {len(html)} chars')
            if 'pubs' in html:
                print('pubs found in HTML')
            else:
                print('pubs NOT in HTML - may be blocked')
        
        await browser.close()

asyncio.run(debug())
