﻿#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksum;
using SevenZip;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Zip工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Zip解压缩帮助类
    /// </summary>
    public class ZipHelper
    {
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        static ZipHelper()
        {
            try
            {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                if (File.Exists(path))
                {
                    SevenZipBase.SetLibraryPath(path);
                }
                else
                {
                    throw new FileLoadException($"{path}文件未找到！");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "初始化7z.dll异常");
            }
        }
        #endregion

        #region Zip压缩
        /// <summary>   
        /// 递归压缩文件夹的内部方法   
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipStream">压缩输出流</param>   
        /// <param name="parentFolderName">此文件夹的上级文件夹</param>   
        /// <returns></returns>   
        private static bool ZipDirectory(string folderToZip, ZipOutputStream zipStream, string parentFolderName)
        {
            var result = true;
            string[] folders, files;
            var crc = new Crc32();
            try
            {
                var ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/"));
                zipStream.PutNextEntry(ent);
                zipStream.Flush();
                files = Directory.GetFiles(folderToZip);
                foreach (var file in files)
                {
                    using (var fs = File.OpenRead(file))
                    {
                        var buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/" + Path.GetFileName(file)));
                        ent.DateTime = DateTime.Now;
                        ent.Size = fs.Length;
                        crc.Reset();
                        crc.Update(buffer);
                        ent.Crc = crc.Value;
                        zipStream.PutNextEntry(ent);
                        zipStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "递归压缩文件夹的内部方法");
                result = false;
            }
            folders = Directory.GetDirectories(folderToZip);
            foreach (var folder in folders)
            {
                if (!ZipDirectory(folder, zipStream, folderToZip)) return false;
            }
            return result;
        }

        /// <summary>   
        /// Zip压缩文件夹    
        /// </summary>   
        /// <param name="folderToZip">要压缩的文件夹路径</param>   
        /// <param name="zipedFile">压缩文件完整路径</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>是否压缩成功</returns>   
        public static bool ZipDirectory(string folderToZip, string zipedFile, string password = null)
        {
            var result = false;
            if (!Directory.Exists(folderToZip)) return result;
            using (var zipStream = new ZipOutputStream(File.Create(zipedFile)))
            {
                zipStream.SetLevel(6);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                result = ZipDirectory(folderToZip, zipStream, "");
            }
            return result;
        }

        /// <summary>
        /// 压缩文件的内部方法
        /// </summary>
        /// <param name="fileToZip">要压缩的文件全名</param>
        /// <param name="zipStream">压缩输出流</param>
        /// <param name="password">密码，默认：null</param>
        /// <returns>压缩结果</returns>
        private static bool ZipFile(string fileToZip, ZipOutputStream zipStream, string password = null)
        {
            var result = true;
            if (!File.Exists(fileToZip)) return false;
            try
            {
                using (var fs = File.OpenRead(fileToZip))
                {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                    var ent = new ZipEntry(Path.GetFileName(fileToZip));
                    zipStream.PutNextEntry(ent);
                    zipStream.SetLevel(6);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "压缩文件");
                result = false;
            }
            return result;
        }

        /// <summary>   
        /// Zip压缩文件   
        /// </summary>   
        /// <param name="fileToZip">要压缩的文件全名</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>压缩结果</returns>   
        public static bool ZipFile(string fileToZip, string zipedFile, string password = null)
        {
            var result = true;
            if (!File.Exists(fileToZip)) return false;
            try
            {
                using (var fs = File.OpenRead(fileToZip))
                {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    using (var f = File.Create(zipedFile))
                    {
                        using (var zipStream = new ZipOutputStream(f))
                        {
                            if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                            var ent = new ZipEntry(Path.GetFileName(fileToZip));
                            zipStream.PutNextEntry(ent);
                            zipStream.SetLevel(6);
                            zipStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "压缩文件");
                result = false;
            }
            return result;
        }

        /// <summary>   
        /// Zip压缩文件或文件夹   
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>压缩结果</returns>   
        public static bool Zip(string fileToZip, string zipedFile, string password = null)
        {
            var result = false;
            if (Directory.Exists(fileToZip))
            {
                result = ZipDirectory(fileToZip, zipedFile, password);
            }
            else if (File.Exists(fileToZip))
            {
                result = ZipFile(fileToZip, zipedFile, password);
            }
            return result;
        }

        /// <summary>
        /// Zip压缩文件或文件夹
        /// </summary>
        /// <param name="filesToZip">要批量压缩的路径或者文件夹</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <param name="password">密码，默认：null</param>
        /// <returns>压缩结果</returns>
        public static bool Zip(List<string> filesToZip, string zipedFile, string password = null)
        {
            var result = true;
            using (var zipStream = new ZipOutputStream(File.Create(zipedFile)))
            {
                zipStream.SetLevel(6);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                filesToZip.ForEach(o =>
                {
                    if (Directory.Exists(o))
                    {
                        if (!ZipDirectory(o, zipStream, "")) result = false;
                    }
                    else if (File.Exists(o))
                    {
                        if (!ZipFile(o, zipStream, password)) result = false;
                    }
                });
            }
            return result;
        }
        #endregion

        #region Zip解压
        /// <summary>   
        /// Zip解压功能(解压压缩文件到指定目录)   
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>   
        /// <param name="password">密码，默认：null</param>   
        /// <returns>解压结果</returns>   
        public static bool UnZip(string fileToUnZip, string zipedFolder, string password = null)
        {
            var result = true;
            ZipEntry ent = null;
            string fileName;
            if (!File.Exists(fileToUnZip)) return false;
            if (!Directory.Exists(zipedFolder)) Directory.CreateDirectory(zipedFolder);
            try
            {
                using (var zipStream = new ZipInputStream(File.OpenRead(fileToUnZip)))
                {
                    if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                    while ((ent = zipStream.GetNextEntry()) != null)
                    {
                        if (!string.IsNullOrEmpty(ent.Name))
                        {
                            fileName = Path.Combine(zipedFolder, ent.Name);
                            fileName = fileName.Replace('/', '\\');
                            if (fileName.EndsWith("\\"))
                            {
                                Directory.CreateDirectory(fileName);
                                continue;
                            }
                            using (var fs = File.Create(fileName))
                            {
                                var size = 2048;
                                var data = new byte[size];
                                while (true)
                                {
                                    size = zipStream.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        fs.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "解压功能(解压压缩文件到指定目录)");
                result = false;
            }
            return result;
        }
        #endregion

        #region 7z压缩
        /// <summary>
        /// 7z压缩
        /// </summary>
        /// <param name="sourcePath">文件或者文件夹路径</param>
        /// <param name="destFileName">压缩后的文件路径</param>
        /// <param name="password">压缩密码</param>
        /// <param name="compressionMode">压缩模式</param>
        /// <param name="outArchiveFormat">压缩格式</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <param name="fileCompressionStarted">压缩开始事件</param>
        /// <param name="filesFound">文件发现事件</param>
        /// <param name="compressing">压缩进度事件</param>
        /// <param name="fileCompressionFinished">压缩完成事件</param>
        /// <returns>压缩结果</returns>
        public static bool CompressTo7z(string sourcePath, string destFileName, string password = null, CompressionMode compressionMode = CompressionMode.Create, OutArchiveFormat outArchiveFormat = OutArchiveFormat.SevenZip, CompressionLevel compressionLevel = CompressionLevel.High, EventHandler<FileNameEventArgs> fileCompressionStarted = null, EventHandler<IntEventArgs> filesFound = null, EventHandler<ProgressEventArgs> compressing = null, EventHandler<EventArgs> fileCompressionFinished = null)
        {
            var result = true;
            try
            {
                var tmp = new SevenZipCompressor
                {
                    CompressionMode = compressionMode,
                    ArchiveFormat = outArchiveFormat,
                    CompressionLevel = compressionLevel
                };
                if (fileCompressionStarted != null)
                {
                    tmp.FileCompressionStarted += fileCompressionStarted;
                }
                if (filesFound != null)
                {
                    tmp.FilesFound += filesFound;
                }
                if (compressing != null)
                {
                    tmp.Compressing += compressing;
                }
                if (fileCompressionFinished != null)
                {
                    tmp.FileCompressionFinished += fileCompressionFinished;
                }
                if (Directory.Exists(sourcePath))
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        tmp.CompressDirectory(sourcePath, destFileName, password);
                    }
                    else
                    {
                        tmp.CompressDirectory(sourcePath, destFileName);
                    }
                }
                else
                {
                    result = false;
                }
                if (File.Exists(sourcePath))
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        tmp.CompressFilesEncrypted(destFileName, password, sourcePath);
                    }
                    else
                    {
                        tmp.CompressFiles(destFileName, sourcePath);
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                LogHelper.Error(ex, "7z压缩文件异常");
            }
            return result;
        }

        /// <summary>
        /// 7z压缩
        /// </summary>
        /// <param name="sourceFiles">文件路径集合</param>
        /// <param name="destFileName">压缩后的文件路径</param>
        /// <param name="password">压缩密码</param>
        /// <param name="compressionMode">压缩模式</param>
        /// <param name="outArchiveFormat">压缩格式</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <param name="fileCompressionStarted">压缩开始事件</param>
        /// <param name="filesFound">文件发现事件</param>
        /// <param name="compressing">压缩进度事件</param>
        /// <param name="fileCompressionFinished">压缩完成事件</param>
        /// <returns>压缩结果</returns>
        public static bool CompressTo7z(List<string> sourceFiles, string destFileName, string password = null, CompressionMode compressionMode = CompressionMode.Create, OutArchiveFormat outArchiveFormat = OutArchiveFormat.SevenZip, CompressionLevel compressionLevel = CompressionLevel.High, EventHandler<FileNameEventArgs> fileCompressionStarted = null, EventHandler<IntEventArgs> filesFound = null, EventHandler<ProgressEventArgs> compressing = null, EventHandler<EventArgs> fileCompressionFinished = null)
        {
            var result = true;
            try
            {
                var tmp = new SevenZipCompressor
                {
                    CompressionMode = compressionMode,
                    ArchiveFormat = outArchiveFormat,
                    CompressionLevel = compressionLevel
                };
                if (fileCompressionStarted != null)
                {
                    tmp.FileCompressionStarted += fileCompressionStarted;
                }
                if (filesFound != null)
                {
                    tmp.FilesFound += filesFound;
                }
                if (compressing != null)
                {
                    tmp.Compressing += compressing;
                }
                if (fileCompressionFinished != null)
                {
                    tmp.FileCompressionFinished += fileCompressionFinished;
                }
                var files = sourceFiles?.Where(o => File.Exists(o))?.ToArray();
                if (files?.Length > 0)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        tmp.CompressFilesEncrypted(destFileName, password, files);
                    }
                    else
                    {
                        tmp.CompressFiles(destFileName, files);
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                LogHelper.Error(ex, "7z压缩文件异常");
            }
            return result;
        }
        #endregion

        #region 7z解压
        /// <summary>
        /// 7z解压缩
        /// </summary>
        /// <param name="filePath">压缩文件路径</param>
        /// <param name="extractFolder">解压缩目录</param>
        /// <param name="password">解压缩密码</param>
        /// <param name="fileExtractionStarted">文件解压缩开始事件</param>
        /// <param name="fileExists">文件存在事件</param>
        /// <param name="extracting">解压进度事件</param>
        /// <param name="fileExtractionFinished">解压缩完成事件</param>
        /// <returns>解压结果</returns>
        public static bool ExtractFrom7z(string filePath, string extractFolder, string password = null, EventHandler<FileInfoEventArgs> fileExtractionStarted = null, EventHandler<FileOverwriteEventArgs> fileExists = null, EventHandler<ProgressEventArgs> extracting = null, EventHandler<FileInfoEventArgs> fileExtractionFinished = null)
        {
            var result = true;
            try
            {
                SevenZipExtractor tmp = null;
                if (!string.IsNullOrEmpty(password))
                {
                    tmp = new SevenZipExtractor(filePath, password);
                }
                else
                {
                    tmp = new SevenZipExtractor(filePath);
                }
                if (fileExtractionStarted != null)
                {
                    tmp.FileExtractionStarted += fileExtractionStarted;
                }
                if (fileExists != null)
                {
                    tmp.FileExists += fileExists;
                }
                if (extracting != null)
                {
                    tmp.Extracting += extracting;
                }
                if (fileExtractionFinished != null)
                {
                    tmp.FileExtractionFinished += fileExtractionFinished;
                }
                tmp.ExtractArchive(extractFolder);
                tmp?.Dispose();
            }
            catch (Exception ex)
            {
                result = false;
                LogHelper.Error(ex, "7z解压缩文件异常");
            }
            return result;
        }
        #endregion
    }
}
