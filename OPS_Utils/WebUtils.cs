using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OPS_Utils
{
    public class WebUtils
    {
        // public FileManagement FileMng = new FileManagement();

        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<String> FileParts { get; set; }

        public WebUtils()
        {
            FileParts = new List<string>();
        }

        /// <summary>
        /// original name + ".part_N.X" (N = file part number, X = total files)
        /// Objective = enumerate files in folder, look for all matching parts of split file. If found, merge and return true.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool MergeFile(string FileName, string Company, string Team, string FileComment, string Userid,
               string lsChBPublic, string lsChBOnlyDepartmen, string lsChBPrivate, string StyleChk)
        {
            bool rslt = false;
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = FileName.Substring(0, FileName.IndexOf(partToken));
            string trailingTokens = FileName.Substring(FileName.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;
            int FileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out FileCount);
            // get a list of all file parts in the temp folder
            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
            string[] FilesList = Directory.GetFiles(Path.GetDirectoryName(FileName), Searchpattern);

            //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
            // only proceed if we have received all the file chunks
            if (FilesList.Count() == FileCount)
            {
                // use a singleton to stop overlapping processes
                if (!MergeFileManager.Instance.InUse(baseFileName))
                {
                    MergeFileManager.Instance.AddFile(baseFileName);
                    if (File.Exists(baseFileName))
                        File.Delete(baseFileName);
                    // add each file located to a list so we can get them into 
                    // the correct order for rebuilding the file
                    List<SortedFile> MergeList = new List<SortedFile>();
                    foreach (string File in FilesList)
                    {
                        SortedFile sFile = new SortedFile();
                        sFile.FileName = File;
                        baseFileName = File.Substring(0, File.IndexOf(partToken));
                        trailingTokens = File.Substring(File.IndexOf(partToken) + partToken.Length);
                        int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
                        sFile.FileOrder = FileIndex;
                        MergeList.Add(sFile);
                    }

                    // sort by the file-part number to ensure we merge back in the correct order
                    var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();

                    // to Generate new FileName in Hdd
                    string GenerateFileName = MediaUnity.GenerateCoupon(15);
                    string Curfilename = MediaUnity.getBetweenString(baseFileName, Company + Team + "\\", baseFileName.Substring(baseFileName.Length - 4).ToString());

                    baseFileName = baseFileName.Replace(Curfilename, GenerateFileName);

                    using (FileStream FS = new FileStream(baseFileName, FileMode.Create))
                    {
                        //string resultExtension = Path.GetExtension(FS);
                        //string resultFileName = FS.FileName;

                        // merge each file chunk back into one contiguous file stream
                        foreach (var chunk in MergeOrder)
                        {
                            try
                            {
                                using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                                {
                                    fileChunk.CopyTo(FS);
                                }
                            }
                            catch
                            {
                                // handle                                
                            }
                        }
                    }

                    foreach (string File in FilesList)
                    {
                        //delete partial
                        System.IO.File.Delete(File);
                    }
                    rslt = true;

                    //record new file upload

                    // unlock the file from singleton
                    MergeFileManager.Instance.RemoveFile(baseFileName);
                }
            }
            return rslt;
        }

        /// <summary>
        /// Vit ADD - 18 July 2017
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Company"></param>
        /// <param name="Team"></param>
        /// <returns></returns>
        public string MergeFile(string FileName, string Company, string Team)
        {
            string rslt = "";
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = FileName.Substring(0, FileName.IndexOf(partToken));
            string trailingTokens = FileName.Substring(FileName.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;
            int FileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out FileCount);
            if (FileIndex == FileCount)
            {
                // get a list of all file parts in the temp folder
                string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
                string[] FilesList = Directory.GetFiles(Path.GetDirectoryName(FileName), Searchpattern);
                //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
                // only proceed if we have received all the file chunks
                MergeFileManager.Instance.AddFile(baseFileName);
                if (File.Exists(baseFileName))
                    File.Delete(baseFileName);
                // add each file located to a list so we can get them into 
                // the correct order for rebuilding the file
                List<SortedFile> MergeList = new List<SortedFile>();
                foreach (string File in FilesList)
                {
                    SortedFile sFile = new SortedFile();
                    sFile.FileName = File;
                    baseFileName = File.Substring(0, File.IndexOf(partToken));
                    trailingTokens = File.Substring(File.IndexOf(partToken) + partToken.Length);
                    int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
                    sFile.FileOrder = FileIndex;
                    MergeList.Add(sFile);
                }
                // sort by the file-part number to ensure we merge back in the correct order
                var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();
                // to Generate new FileName in Hdd
                string GenerateFileName = MediaUnity.GenerateCoupon(15);
                string Curfilename = MediaUnity.getBetweenString(baseFileName, Company + Team + "\\", baseFileName.Substring(baseFileName.Length - 4).ToString());
                baseFileName = baseFileName.Replace(Curfilename, GenerateFileName);
                using (FileStream FS = new FileStream(baseFileName, FileMode.Create))
                {
                    // merge each file chunk back into one contiguous file stream
                    foreach (var chunk in MergeOrder)
                    {
                        try
                        {
                            using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                            {
                                fileChunk.CopyTo(FS);
                            }
                        }
                        catch
                        {
                            // handle                                
                        }
                    }
                }
                foreach (string File in FilesList)
                {
                    System.IO.File.Delete(File);
                }
                rslt = baseFileName;
                MergeFileManager.Instance.RemoveFile(baseFileName);
            }
            return rslt;
        }

        /// <summary>
        /// SON ADD - 18 July 2017
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string MergeFile(string fileName, string sysFileName)
        {
            string rslt = "";
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = fileName.Substring(0, fileName.IndexOf(partToken));
            string trailingTokens = fileName.Substring(fileName.IndexOf(partToken) + partToken.Length);
            int fileIndex = 0;
            int fileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out fileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out fileCount);
            if (fileIndex == fileCount)
            {
                // get a list of all file parts in the temp folder
                string searchPattern = Path.GetFileName(baseFileName) + partToken + "*";
                string[] filesList = Directory.GetFiles(Path.GetDirectoryName(fileName), searchPattern);
                //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
                // only proceed if we have received all the file chunks
                MergeFileManager.Instance.AddFile(baseFileName);
                if (File.Exists(baseFileName))
                    File.Delete(baseFileName);
                // add each file located to a list so we can get them into 
                // the correct order for rebuilding the file
                List<SortedFile> MergeList = new List<SortedFile>();
                foreach (string file in filesList)
                {
                    SortedFile sFile = new SortedFile();
                    sFile.FileName = file;
                    baseFileName = file.Substring(0, file.IndexOf(partToken));
                    trailingTokens = file.Substring(file.IndexOf(partToken) + partToken.Length);
                    int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out fileIndex);
                    sFile.FileOrder = fileIndex;
                    MergeList.Add(sFile);
                }
                // sort by the file-part number to ensure we merge back in the correct order
                var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();

                // to Generate new FileName in Hdd
                //string generateFileName = MediaUnity.GenerateCoupon(15);

                var extFile = Path.GetExtension(baseFileName);
                var getDirectory = Path.GetDirectoryName(baseFileName) + "/";
                baseFileName = getDirectory + sysFileName + extFile;

                //Check file is exist or not
                if (File.Exists(baseFileName))
                {
                    File.Delete(baseFileName);
                }

                using (FileStream fs = new FileStream(baseFileName, FileMode.Create))
                {
                    // merge each file chunk back into one contiguous file stream
                    foreach (var chunk in MergeOrder)
                    {
                        try
                        {
                            using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                            {
                                fileChunk.CopyTo(fs);
                            }
                        }
                        catch
                        {
                            // handle                                
                        }
                    }
                }
                foreach (string file in filesList)
                {
                    File.Delete(file);
                }
                rslt = baseFileName;
                MergeFileManager.Instance.RemoveFile(baseFileName);
            }
            return rslt;
        }

        public string OpsMergeFile(string fileName, string sysFileName)
        {
            string rslt = "";
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = fileName.Substring(0, fileName.IndexOf(partToken));
            string trailingTokens = fileName.Substring(fileName.IndexOf(partToken) + partToken.Length);
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out var fileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out var fileCount);
            if (fileIndex == fileCount)
            {
                // get a list of all file parts in the temp folder
                string searchPattern = Path.GetFileName(baseFileName) + partToken + "*";
                string[] filesList = Directory.GetFiles(Path.GetDirectoryName(fileName), searchPattern);
                //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
                // only proceed if we have received all the file chunks
                MergeFileManager.Instance.AddFile(baseFileName);
                if (File.Exists(baseFileName))
                    File.Delete(baseFileName);
                // add each file located to a list so we can get them into 
                // the correct order for rebuilding the file
                List<SortedFile> MergeList = new List<SortedFile>();
                foreach (string file in filesList)
                {
                    SortedFile sFile = new SortedFile();
                    sFile.FileName = file;
                    baseFileName = file.Substring(0, file.IndexOf(partToken));
                    trailingTokens = file.Substring(file.IndexOf(partToken) + partToken.Length);
                    int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out fileIndex);
                    sFile.FileOrder = fileIndex;
                    MergeList.Add(sFile);
                }
                // sort by the file-part number to ensure we merge back in the correct order
                var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();
                var getDirectory = $"{Path.GetDirectoryName(baseFileName)}";
                //baseFileName = getDirectory + sysFileName;
                baseFileName = Path.Combine(getDirectory, sysFileName);

                //Check file is exist or not
                if (File.Exists(baseFileName))
                {
                    File.Delete(baseFileName);
                }

                using (FileStream fs = new FileStream(baseFileName, FileMode.Create))
                {
                    // merge each file chunk back into one contiguous file stream
                    foreach (var chunk in MergeOrder)
                    {
                        try
                        {
                            using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                            {
                                fileChunk.CopyTo(fs);
                            }
                        }
                        catch
                        {
                            // handle                                
                        }
                    }
                }
                foreach (string file in filesList)
                {
                    File.Delete(file);
                }
                rslt = baseFileName;
                MergeFileManager.Instance.RemoveFile(baseFileName);
            }
            return rslt;
        }

        /// <summary>
        /// Uploader Delete File
        /// </summary>
        /// <param name="uflt"></param>
        /// <returns></returns>
        public bool DeleteUserFile(string PathFile)
        {
            var filePath = Path.GetDirectoryName(PathFile);

            var fileName = Path.GetFileName(PathFile);

            string destFile = System.IO.Path.Combine(filePath, fileName);
            try
            {
                //delete partial
                System.IO.File.Delete(destFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public struct SortedFile
    {
        public int FileOrder { get; set; }
        public String FileName { get; set; }
    }
    //===================

    public class MergeFileManager
    {
        private static MergeFileManager instance;
        private List<string> MergeFileList;
        private MergeFileManager()
        {
            try
            {
                MergeFileList = new List<string>();
            }
            catch { }
        }

        public static MergeFileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MergeFileManager();
                return instance;
            }
        }

        public void AddFile(string BaseFileName)
        {
            MergeFileList.Add(BaseFileName);
        }

        public bool InUse(string BaseFileName)
        {
            return MergeFileList.Contains(BaseFileName);
        }

        public bool RemoveFile(string BaseFileName)
        {
            return MergeFileList.Remove(BaseFileName);
        }
    }
}
