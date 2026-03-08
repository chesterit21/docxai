Element button Upload Z_AI:

<button id="upload-file-button" class="rounded-lg bg-transparent transition p-1 outline-hidden focus:outline-hidden hover:bg-gray-100 text-gray-800 dark:text-white dark:hover:bg-gray-800" type="button" aria-label="More"><svg class="size-5" width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M10.0001 4.16675V15.8334M4.16675 10.0001H15.8334" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"></path></svg></button>

Veris terbau rnya, dia hampir sam aseperti Deepseek, gak ada klik menu dulu kayak Qwen....
Jadi ketika di klik button tersebut, langsung terbuka Explorer untuk pilih file...

dan ini Method sebelum nya :

    public static async Task UploadFileViaDialog(IPage page, string filePath)
    {
        string finalFilePath = filePath;
        bool isRenamed = false;
        string buttonUploadSelector = await EventPageHandler.FindSelectorByOtherNode(page, KeySelector, ButtonOpenFileUpload);

        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            if (FileTypeHelper.NeedsRename(filePath))
            {
                Console.WriteLine($"[Upload] 🔄 Programming file detected, creating renamed copy...");
                finalFilePath = FileTypeHelper.CreateRenamedCopy(filePath);
                isRenamed = true;
            }
            else
            {
                Console.WriteLine($"[Upload] ✅ File type allowed, using original file");
            }

            
            // Strategy 1: Direct File Input
            var fileInput = page.Locator("input[type='file']");
            if (await fileInput.CountAsync() > 0)
            {
                await fileInput.SetInputFilesAsync(finalFilePath);
                await page.WaitForTimeoutAsync(1000);
                return;
            }

            // Strategy 2: Text-Based Selector
            var uploadTexts = new[] { "Add File", "Upload", "Select File", "Choose File" };
            foreach (var text in uploadTexts)
            {
                var textButton = page.GetByText(text, new PageGetByTextOptions { Exact = false });
                if (await textButton.CountAsync() > 0)
                {
                    await textButton.First.ClickAsync();
                    var fileChooser = await page.RunAndWaitForFileChooserAsync(
                        () => textButton.First.ClickAsync(),
                        new PageRunAndWaitForFileChooserOptions { Timeout = 8000 });
                    await fileChooser.SetFilesAsync(finalFilePath);
                    return;
                }
            }

            // Strategy 3: Original Selector
            var buttonLocator = page.Locator(buttonUploadSelector);

            await buttonLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = 15000
            });

            if (!await buttonLocator.IsVisibleAsync())
            {
                await buttonLocator.ScrollIntoViewIfNeededAsync();
                await page.WaitForTimeoutAsync(2000);
            }

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    var fileChooser = await page.RunAndWaitForFileChooserAsync(
                        () => buttonLocator.ClickAsync(new LocatorClickOptions
                        {
                            Force = attempt == 2,
                            Timeout = 5000
                        }),
                        new PageRunAndWaitForFileChooserOptions { Timeout = 10000 });

                    await fileChooser.SetFilesAsync(finalFilePath);
                    return;
                }
                catch (TimeoutException)
                {
                    if (attempt == 1)
                    {
                        Console.WriteLine("[Upload] ⏳ First attempt timeout, retrying...");
                        await page.WaitForTimeoutAsync(3000);
                        continue;
                    }
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Upload] ❌ Upload failed: {ex.Message}");
            throw;
        }
        finally
        {
            if (isRenamed && finalFilePath != filePath)
            {
                try
                {
                    FileTypeHelper.CleanupRenamedFile(finalFilePath);
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"[Upload] ⚠️ Cleanup warning: {cleanupEx.Message}");
                }
            }
        }
    }
