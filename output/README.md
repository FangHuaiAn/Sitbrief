# Sitbrief Output

此目錄存放匯出的 JSON 檔案，供 iOS App 使用。

檔案：
- `articles.json` - 所有文章資料
- `topics.json` - 所有主題資料  
- `metadata.json` - 同步元資料

使用 `@sitbrief ExportJson` 指令產生這些檔案。
使用 `@sitbrief sync` 指令上傳到 Azure Static Web Apps。
