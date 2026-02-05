/**
 * Sitbrief API Gateway - Cloudflare Worker
 * 
 * 負責：
 * 1. 驗證 iOS App 的 Access Token
 * 2. 代理 R2 檔案存取（支援分頁結構）
 * 3. 設定 CORS 標頭
 */

interface Env {
	// R2 Bucket 綁定
	SITBRIEF_BUCKET: R2Bucket;
	// 環境變數
	ACCESS_TOKEN: string;
}

export default {
	async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
		// 處理 CORS preflight
		if (request.method === 'OPTIONS') {
			return handleCORS();
		}

		const url = new URL(request.url);
		const path = url.pathname;

		// Health check（不需要認證）
		if (path === '/' || path === '/health') {
			return jsonResponse({ 
				status: 'ok', 
				service: 'Sitbrief API Gateway',
				version: '2.0.0'
			});
		}

		// API 路由（需要認證）
		if (path.startsWith('/api/')) {
			return handleAPIRequest(request, env, path);
		}

		return jsonResponse({ error: 'Not Found' }, 404);
	},
};

/**
 * 處理 API 請求
 */
async function handleAPIRequest(request: Request, env: Env, path: string): Promise<Response> {
	// 驗證 Token
	const authResult = validateToken(request, env);
	if (!authResult.valid) {
		return jsonResponse({ error: authResult.error }, 401);
	}

	// 路由處理
	
	// GET /api/metadata
	if (path === '/api/metadata') {
		return serveFile(env, 'Brief/metadata.json');
	}
	
	// GET /api/topics
	if (path === '/api/topics') {
		return serveFile(env, 'Brief/topics.json');
	}
	
	// GET /api/articles/latest
	if (path === '/api/articles/latest') {
		return serveFile(env, 'Brief/articles/latest.json');
	}
	
	// GET /api/articles/page/:page
	const pageMatch = path.match(/^\/api\/articles\/page\/(\d+)$/);
	if (pageMatch) {
		const page = pageMatch[1];
		return serveFile(env, `Brief/articles/page-${page}.json`);
	}

	// 向後兼容：舊的 /api/articles 端點
	if (path === '/api/articles') {
		return serveFile(env, 'Brief/articles/latest.json');
	}

	return jsonResponse({ error: 'Not Found' }, 404);
}

/**
 * 驗證 Access Token
 */
function validateToken(request: Request, env: Env): { valid: boolean; error?: string } {
	const authHeader = request.headers.get('Authorization');
	
	if (!authHeader) {
		return { valid: false, error: 'Missing Authorization header' };
	}

	// 支援兩種格式：
	// 1. Bearer <token>
	// 2. <token>
	const token = authHeader.startsWith('Bearer ') 
		? authHeader.substring(7)
		: authHeader;

	if (token !== env.ACCESS_TOKEN) {
		return { valid: false, error: 'Invalid token' };
	}

	return { valid: true };
}

/**
 * 從 R2 讀取並返回檔案
 */
async function serveFile(env: Env, key: string): Promise<Response> {
	const object = await env.SITBRIEF_BUCKET.get(key);

	if (!object) {
		return jsonResponse({ error: 'File not found', key: key }, 404);
	}

	const headers = new Headers();
	headers.set('Content-Type', 'application/json');
	headers.set('Cache-Control', 'public, max-age=300'); // 5 分鐘快取
	addCORSHeaders(headers);

	// 讀取完整內容
	const content = await object.text();

	return new Response(content, {
		status: 200,
		headers: headers
	});
}

/**
 * 處理 CORS preflight
 */
function handleCORS(): Response {
	const headers = new Headers();
	addCORSHeaders(headers);
	headers.set('Access-Control-Max-Age', '86400'); // 24 小時

	return new Response(null, {
		status: 204,
		headers: headers
	});
}

/**
 * 新增 CORS 標頭
 */
function addCORSHeaders(headers: Headers): void {
	headers.set('Access-Control-Allow-Origin', '*');
	headers.set('Access-Control-Allow-Methods', 'GET, OPTIONS');
	headers.set('Access-Control-Allow-Headers', 'Authorization, Content-Type');
}

/**
 * JSON 回應輔助函數
 */
function jsonResponse(data: any, status: number = 200): Response {
	const headers = new Headers();
	headers.set('Content-Type', 'application/json');
	addCORSHeaders(headers);

	return new Response(JSON.stringify(data), {
		status: status,
		headers: headers
	});
}
