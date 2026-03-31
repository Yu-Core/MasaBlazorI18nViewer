window.chooseFileSystemFolder = async function () {
    try {
        const handle = await window.showDirectoryPicker({
            mode: 'readwrite'
        });
        return {
            getFiles: async (fileExtension) => {
                const results = [];
                for await (const entry of handle.values()) {
                    if (entry.kind === 'file') {
                        if (!fileExtension || entry.name.endsWith(fileExtension)) {
                            const file = await entry.getFile();
                            const content = await file.text();
                            results.push({ fileName: entry.name, content: content });
                        }
                    }
                }
                return results;
            },
            saveFiles: async (files) => {
                await Promise.all(files.map(async (f) => {
                    const fileName = f.fileName || f.FileName;
                    const newContent = f.content || f.Content;
                    if (!fileName || newContent == null) return; // 跳过无效项

                    let targetEOL = '\n'; // 默认换行符
                    let fileHandle = null;

                    // 尝试获取已存在的文件句柄（不自动创建）
                    try {
                        fileHandle = await handle.getFileHandle(fileName);
                        const file = await fileHandle.getFile();
                        const originalContent = await file.text();
                        targetEOL = detectLineEnding(originalContent);
                    } catch (err) {
                        if (err.name !== 'NotFoundError') throw err;
                        // 文件不存在，使用默认换行符（保留原系统行为）
                    }

                    // 规范化新内容
                    const normalizedContent = normalizeLineEndings(newContent, targetEOL);

                    // 创建或覆盖文件并写入
                    const writeHandle = fileHandle || await dirHandle.getFileHandle(fileName, { create: true });
                    const writable = await writeHandle.createWritable();
                    await writable.write(normalizedContent);
                    await writable.close();
                }));

                // 检测文本内容的换行符风格（优先级: CRLF > LF > CR）
                function detectLineEnding(content) {
                    if (content.includes('\r\n')) return '\r\n';
                    if (content.includes('\n')) return '\n';
                    if (content.includes('\r')) return '\r';
                    return '\n'; // 默认
                }

                // 将文本内容统一转换为目标换行符
                function normalizeLineEndings(content, targetEOL) {
                    // 先将所有常见换行符统一为 \n，再替换为目标格式
                    return content.replace(/\r\n|\n|\r/g, '\n').replace(/\n/g, targetEOL);
                }
            }
        };
    } catch (e) {
        return null;
    }
};
