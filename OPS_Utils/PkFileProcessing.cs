using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OPS_Utils
{
    public class PkFileProcessing
    {
        protected int GetTotalFilePartOfChunk(string chunkFileName)
        {
            var lst = chunkFileName.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var lastPart = lst.LastOrDefault();
            int.TryParse(lastPart, out var totalPart);

            return totalPart;
        }

        protected string GetFileNameByChunkFileName(string chunkFileName)
        {
            var lastDotIndex = chunkFileName.LastIndexOf('.');
            return chunkFileName.Substring(0, lastDotIndex);
        }

        public void ConvertVideo(string inputFilePath, string outputFilePath)
        {
            var logo = @"~/assets/ops-fileupload/temp/vdlogo.png";
            var ffmpegFilePath = Path.GetPathRoot("~/assets/ffmpeg/bin/ffmpeg.exe");
            var strCmdText = $@"-i {inputFilePath} -i {logo} -filter_complex ""overlay=(main_w-overlay_w-5):5"" -codec:a copy {outputFilePath}";
            var process = System.Diagnostics.Process.Start(ffmpegFilePath, strCmdText);
            process?.WaitForExit();
        }

        public async Task<bool> CheckAndMergeChunkFilesAsync(string oneChunkFilePath, string destinationFilePath, string catalog)
        {
            var chunkFileInfo = new FileInfo(oneChunkFilePath);
            var directoryInfo = chunkFileInfo.Directory;
            var baseFolderPath = directoryInfo?.FullName;
            var totalPart = GetTotalFilePartOfChunk(oneChunkFilePath);
            var fileNameWithoutChunkExtension = GetFileNameByChunkFileName(chunkFileInfo.Name);
            var lstFile = directoryInfo?.GetFiles();

            if (lstFile == null || lstFile.Length < totalPart) return false;

            var lstChunkFilePaths = new List<string>();

            //check all part completed
            for (int i = 1; i <= totalPart; i++)
            {
                var chunkFileName = $@"{fileNameWithoutChunkExtension}.part{i}-{totalPart}";
                var chunkFilePath = Path.Combine(baseFolderPath, chunkFileName);
                if (File.Exists(chunkFilePath) == false)//lack file
                    return false;

                lstChunkFilePaths.Add(chunkFilePath);
            }

            var tempDestinationPath = $@"{destinationFilePath}.temp";

            //merge
            using (var outputStream = File.Create(tempDestinationPath))
            {
                foreach (var chunkFilePath in lstChunkFilePaths)
                {
                    using (var chunkStream = File.OpenRead(chunkFilePath))
                    {
                        // Buffer size can be passed as the second argument.
                        await chunkStream.CopyToAsync(outputStream);
                    }
                }
            }

            //delete all chunk files
            foreach (var chunkFilePath in lstChunkFilePaths)
            {
                try
                {
                    File.Delete(chunkFilePath);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (catalog == "video")
            {
                //convert to viewable format
                ConvertVideo(tempDestinationPath, destinationFilePath);
            }
            else
            {
                //convert to viewable format
                File.Copy(tempDestinationPath, destinationFilePath, true);
            }

            //delete temp file
            File.Delete(tempDestinationPath);

            return true;
        }

        public async Task<string> OpsMergeFile(string fileName, string sysFilePath)
        {
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = fileName.Substring(0, fileName.IndexOf(partToken, StringComparison.Ordinal));
            string trailingTokens = fileName.Substring(fileName.IndexOf(partToken, StringComparison.Ordinal) + partToken.Length);

            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".", StringComparison.Ordinal)), out var fileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".", StringComparison.Ordinal) + 1), out var fileCount);

            var chunkFileInfo = new FileInfo(fileName);
            var dirInfo = chunkFileInfo.Directory;
            var chuckFiles = dirInfo?.GetFiles();
            var totalChunkFilesInDir = 0;

            if (chuckFiles != null) totalChunkFilesInDir = chuckFiles.Length;

            if (totalChunkFilesInDir != fileCount) return "";

            // Merging chunk files
            // get a list of all file parts in the temp folder
            string searchPattern = Path.GetFileName(baseFileName) + partToken + "*";
            string[] filesList = Directory.GetFiles(Path.GetDirectoryName(fileName) ?? throw new InvalidOperationException(), searchPattern);

            //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
            // only proceed if we have received all the file chunks
            MergeFileManager.Instance.AddFile(baseFileName);
            if (File.Exists(baseFileName)) File.Delete(baseFileName);

            // add each file located to a list so we can get them into 
            // the correct order for rebuilding the file
            var mergeList = new List<SortedFile>();
            foreach (string file in filesList)
            {
                SortedFile sFile = new SortedFile { FileName = file };
                baseFileName = file.Substring(0, file.IndexOf(partToken, StringComparison.Ordinal));
                trailingTokens = file.Substring(file.IndexOf(partToken, StringComparison.Ordinal) + partToken.Length);
                int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".", StringComparison.Ordinal)), out fileIndex);
                sFile.FileOrder = fileIndex;
                mergeList.Add(sFile);
            }

            // sort by the file-part number to ensure we merge back in the correct order
            var mergeOrder = mergeList.OrderBy(s => s.FileOrder).ToList();
            
            //Check file is exist or not
            if (File.Exists(sysFilePath)) File.Delete(sysFilePath);

            // Merging chunk files from temp to upload folder
            using (var fs = new FileStream(sysFilePath, FileMode.Create))
            {
                // merge each file chunk back into one contiguous file stream
                foreach (var chunk in mergeOrder)
                {
                    using (var fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                    {
                        await fileChunk.CopyToAsync(fs);
                    }
                }
            }
            foreach (var file in filesList)
            {
                File.Delete(file);
            }
            MergeFileManager.Instance.RemoveFile(baseFileName);

            return sysFilePath;
        }
    }
}
