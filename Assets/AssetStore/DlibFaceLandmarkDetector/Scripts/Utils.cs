using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace DlibFaceLandmarkDetector.UnityUtils
{
    public static class Utils
    {
        /**
        * Returns this "Dlib FaceLandmark Detector" version number.
        * 
        * @return this "Dlib FaceLandmark Detector" version number
        */
        public static string getVersion()
        {
            return "1.3.8";
        }

        /**
        * Gets the readable path of a file in the "StreamingAssets" folder.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder. e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] If the target file has not yet been copied to WebGL's virtual filesystem, you need to use getFilePathAsync() at first.
        * 
        * @param filepath a relative file path starting from "StreamingAssets" folder.
        * @param refresh [Android] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns a readable file path in case of success and returns empty in case of error.
        */
        public static string getFilePath(string filepath, bool refresh = false, int timeout = 0)
        {
            if (filepath == null)
                filepath = string.Empty;

            filepath = filepath.TrimStart(chTrims);

            if (string.IsNullOrEmpty(filepath) || string.IsNullOrEmpty(Path.GetExtension(filepath)))
                return String.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            string srcPath = Path.Combine(Application.streamingAssetsPath, filepath);
            string destPath = Path.Combine(Application.persistentDataPath, "dlibfacelandmarkdetector");
            destPath = Path.Combine(destPath, filepath);

            if (!refresh && File.Exists(destPath))
                return destPath;

#if UNITY_2017_1_OR_NEWER
            using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(srcPath))
            {
                request.timeout = timeout;

#if UNITY_2018_2_OR_NEWER
                request.SendWebRequest ();
#else
                request.Send();
#endif

                while (!request.isDone) {; }

#if UNITY_2020_2_OR_NEWER
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityEngine.Networking.UnityWebRequest.Result.DataProcessingError)
                {
#elif UNITY_2017_1_OR_NEWER
                if (request.isHttpError || request.isNetworkError) 
                {
#else
                if (request.isError)
                {
#endif
                    Debug.LogWarning(request.error);
                    Debug.LogWarning(request.responseCode);
                    return String.Empty;
                }

                //create Directory
                String dirPath = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.WriteAllBytes(destPath, request.downloadHandler.data);
            }
#else // UNITY_2017_1_OR_NEWER
            using (WWW request = new WWW(srcPath))
            {
                while (!request.isDone) {; }

                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogWarning(request.error);
                    return String.Empty;
                }

                //create Directory
                String dirPath = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.WriteAllBytes(destPath, request.bytes);
            }
#endif // UNITY_2017_1_OR_NEWER
            return destPath;
#elif UNITY_WEBGL && !UNITY_EDITOR
            string destPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), "dlibfacelandmarkdetector");
            destPath = Path.Combine(destPath, filepath);

            if (File.Exists(destPath))
            {
                return destPath;
            }
            else
            {
                return String.Empty;
            }
#else // #if UNITY_ANDROID && !UNITY_EDITOR
            string destPath = Path.Combine(Application.streamingAssetsPath, filepath);

            if (File.Exists(destPath))
            {
                return destPath;
            }
            else
            {
                return String.Empty;
            }
#endif // #if UNITY_ANDROID && !UNITY_EDITOR
        }

        /**
        * Gets the multiple readable paths of files in the "StreamingAssets" folder.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder. e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] If the target file has not yet been copied to WebGL's virtual filesystem, you need to use getFilePathAsync() at first.
        * 
        * @param filepaths a list of relative file paths starting from the "StreamingAssets" folder.
        * @param refresh [Android] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns a list of readable file paths. Returns a readable file path in case of success and returns empty in case of error.
        */
        public static List<string> getMultipleFilePaths(IList<string> filepaths, bool refresh = false, int timeout = 0)
        {
            if (filepaths == null)
                throw new ArgumentNullException("filepaths");

            List<string> result = new List<string>();

            for (int i = 0; i < filepaths.Count; i++)
            {
                result.Add(getFilePath(filepaths[i], refresh, timeout));
            }

            return result;
        }

        /**
        * Gets the readable path of a file in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepath a relative file path starting from the "StreamingAssets" folder.
        * @param completed a callback function that is called when the process is completed. Returns a readable file path in case of success and returns empty in case of error.
        * @param refresh [Android][WebGL] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getFilePathAsync(string filepath, Action<string> completed, bool refresh = false, int timeout = 0)
        {
            return getFilePathAsync(filepath, completed, null, null, refresh, timeout);
        }

        /**
        * Gets the readable path of a file in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepath a relative file path starting from the "StreamingAssets" folder.
        * @param completed a callback function that is called when the process is completed. Returns a readable file path in case of success and returns empty in case of error.
        * @param progressChanged a callback function that is called when the process is the progress. Returns the file path and a progress value (0.0 to 1.0).
        * @param refresh [Android][WebGL] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getFilePathAsync(string filepath, Action<string> completed, Action<string, float> progressChanged, bool refresh = false, int timeout = 0)
        {
            return getFilePathAsync(filepath, completed, progressChanged, null, refresh, timeout);
        }

        /**
        * Gets the readable path of a file in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepath a relative file path starting from the "StreamingAssets" folder.
        * @param completed a callback function that is called when the process is completed. Returns a readable file path in case of success and returns empty in case of error.
        * @param progressChanged a callback function that is called when the process is the progress. Returns the file path and a progress value (0.0 to 1.0).
        * @param errorOccurred a callback function that is called when the process is error occurred. Returns the file path and an error string and an error response code.
        * @param refresh [Android][WebGL] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getFilePathAsync(string filepath, Action<string> completed, Action<string, float> progressChanged, Action<string, string, long> errorOccurred, bool refresh = false, int timeout = 0)
        {
            if (filepath == null)
                filepath = string.Empty;

            filepath = filepath.TrimStart(chTrims);

            if (string.IsNullOrEmpty(filepath) || string.IsNullOrEmpty(Path.GetExtension(filepath)))
            {
                if (progressChanged != null)
                    progressChanged(filepath, 0);
                yield return null;
                if (progressChanged != null)
                    progressChanged(filepath, 1);

                if (errorOccurred != null)
                {
                    errorOccurred(filepath, "Invalid file path.", 0);
                }
                else
                {
                    if (completed != null)
                        completed(String.Empty);
                }
                yield break;
            }

#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
            string srcPath = Path.Combine(Application.streamingAssetsPath, filepath);
#if UNITY_ANDROID
            string destPath = Path.Combine(Application.persistentDataPath, "dlibfacelandmarkdetector");
#else
            string destPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), "dlibfacelandmarkdetector");
#endif
            destPath = Path.Combine(destPath, filepath);

            if (!refresh && File.Exists(destPath))
            {
                if (progressChanged != null)
                    progressChanged(filepath, 0);
                yield return null;
                if (progressChanged != null)
                    progressChanged(filepath, 1);
                if (completed != null)
                    completed(destPath);
            }
            else
            {

#if UNITY_WEBGL || (UNITY_ANDROID && UNITY_2017_1_OR_NEWER)
                using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(srcPath))
                {
                    request.timeout = timeout;

#if UNITY_2018_2_OR_NEWER
                    request.SendWebRequest ();
#else
                    request.Send();
#endif

                    while (!request.isDone)
                    {

                        if (progressChanged != null)
                            progressChanged(filepath, request.downloadProgress);

                        yield return null;
                    }

                    if (progressChanged != null)
                        progressChanged(filepath, request.downloadProgress);

#if UNITY_2020_2_OR_NEWER
                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError ||
                        request.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityEngine.Networking.UnityWebRequest.Result.DataProcessingError)
                    {
#elif UNITY_2017_1_OR_NEWER
                    if (request.isHttpError || request.isNetworkError) 
                    {
#else
                    if (request.isError)
                    {
#endif
                        Debug.LogWarning(request.error);
                        Debug.LogWarning(request.responseCode);

                        if (errorOccurred != null)
                        {
                            errorOccurred(filepath, request.error, request.responseCode);
                        }
                        else
                        {
                            if (completed != null)
                                completed(String.Empty);
                        }
                        yield break;
                    }

                    //create Directory
                    String dirPath = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    File.WriteAllBytes(destPath, request.downloadHandler.data);
                }
#else // UNITY_WEBGL || (UNITY_ANDROID && UNITY_2017_1_OR_NEWER)
                using (WWW request = new WWW(srcPath))
                {

                    while (!request.isDone)
                    {
                        if (progressChanged != null)
                            progressChanged(filepath, request.progress);

                        yield return null;
                    }

                    if (progressChanged != null)
                        progressChanged(filepath, request.progress);

                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);

                        if (errorOccurred != null)
                        {
                            errorOccurred(filepath, request.error, 0);
                        }
                        else
                        {
                            if (completed != null)
                                completed(String.Empty);
                        }
                        yield break;
                    }

                    //create Directory
                    String dirPath = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    File.WriteAllBytes(destPath, request.bytes);
                }
#endif // UNITY_WEBGL || (UNITY_ANDROID && UNITY_2017_1_OR_NEWER)
                if (completed != null)
                    completed(destPath);
            }
#else // (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
            string destPath = Path.Combine(Application.streamingAssetsPath, filepath);

            if (progressChanged != null)
                progressChanged(filepath, 0);
            yield return null;
            if (progressChanged != null)
                progressChanged(filepath, 1);

            if (File.Exists(destPath))
            {
                if (completed != null)
                    completed(destPath);
            }
            else
            {
                if (errorOccurred != null)
                {
                    errorOccurred(filepath, "File does not exist.", 0);
                }
                else
                {
                    if (completed != null)
                        completed(String.Empty);
                }
            }
#endif // (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR

            yield break;
        }

        /**
        * Gets the multiple readable paths of files in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepaths a list of relative file paths starting from the "StreamingAssets" folder.
        * @param allCompleted a callback function that is called when all processes are completed. Returns a list of file paths. Returns a readable file path in case of success and returns empty in case of error.
        * @param refresh [Android][WebGL] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getMultipleFilePathsAsync(IList<string> filepaths, Action<List<string>> allCompleted, bool refresh = false, int timeout = 0)
        {
            return getMultipleFilePathsAsync(filepaths, allCompleted, null, null, null, refresh, timeout);
        }

        /**
        * Gets the multiple readable paths of files in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepaths a list of relative file paths starting from the "StreamingAssets" folder.
        * @param allCompleted a callback function that is called when all processes are completed. Returns a list of file paths. Returns a readable file path in case of success and returns empty in case of error.
        * @param completed a callback function that is called when one process is completed. Returns a readable file path in case of success and returns empty in case of error.
        * @param refresh [Android][WebGL] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getMultipleFilePathsAsync(IList<string> filepaths, Action<List<string>> allCompleted, Action<string> completed, bool refresh = false, int timeout = 0)
        {
            return getMultipleFilePathsAsync(filepaths, allCompleted, completed, null, null, refresh, timeout);
        }

        /**
        * Gets the multiple readable paths of files in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepaths a list of relative file paths starting from the "StreamingAssets" folder.
        * @param allCompleted a callback function that is called when all processes are completed. Returns a list of file paths. Returns a readable file path in case of success and returns empty in case of error.
        * @param completed a callback function that is called when one process is completed. Returns a readable file path in case of success and returns empty in case of error.
        * @param progressChanged a callback function that is called when one process is the progress. Returns the file path and a progress value (0.0 to 1.0).
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getMultipleFilePathsAsync(IList<string> filepaths, Action<List<string>> allCompleted, Action<string> completed, Action<string, float> progressChanged, bool refresh = false, int timeout = 0)
        {
            return getMultipleFilePathsAsync(filepaths, allCompleted, completed, progressChanged, null, refresh, timeout);
        }

        /**
        * Gets the multiple readable paths of files in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android] The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * <br>[WebGL] The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied. 
        * 
        * @param filepaths a list of relative file paths starting from the "StreamingAssets" folder.
        * @param allCompleted a callback function that is called when all processes are completed. Returns a list of file paths. Returns a readable file path in case of success and returns empty in case of error.
        * @param completed a callback function that is called when one process is completed. Returns a readable file path in case of success and returns empty in case of error.
        * @param progressChanged a callback function that is called when one process is the progress. Returns the file path and a progress value (0.0 to 1.0).
        * @param errorOccurred a callback function that is called when one process is error occurred. Returns the file path and an error string and an error response code.
        * @param refresh [Android][WebGL] If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copied.
        * @param timeout [Android 2017.1+][WebGL] Sets UnityWebRequest to attempt to abort after the number of seconds in timeout has passed. No timeout is applied when timeout is set to 0 and this property defaults to 0. 
        * @return returns an IEnumerator object. Yielding the IEnumerator inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
        */
        public static IEnumerator getMultipleFilePathsAsync(IList<string> filepaths, Action<List<string>> allCompleted, Action<string> completed, Action<string, float> progressChanged, Action<string, string, long> errorOccurred, bool refresh = false, int timeout = 0)
        {
            if (filepaths == null)
                throw new ArgumentNullException("filepaths");

            List<string> readableFilePaths = new List<string>();

            for (int i = 0; i < filepaths.Count; i++)
            {
                yield return getFilePathAsync(filepaths[i],
                (path) =>
                {
                    readableFilePaths.Add(path);

                    if (completed != null)
                        completed(path);
                },
                progressChanged,
                (path, error, code) =>
                {
                    readableFilePaths.Add(string.Empty);

                    if (errorOccurred != null)
                        errorOccurred(path, error, code);
                }
                , refresh, timeout);
            }

            if (allCompleted != null)
                allCompleted(readableFilePaths);
        }

        private static char[] chTrims = {
            '.',
            #if UNITY_WINRT_8_1 && !UNITY_EDITOR
            '/',
            '\\'
            #else
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar
            #endif
        };


#pragma warning disable 0414
        /// <summary>
        /// if true, DlibException is thrown instead of calling Debug.LogError (msg).
        /// </summary>
        private static bool throwDlibException = false;

        /// <summary>
        /// callback callback called when an Dlib error occurs on the Native side.
        /// </summary>
        private static Action<string> dlibSetDebugModeCallback;
#pragma warning restore 0414

        /**
        * Sets the debug mode.
        * <p>
        * <br>if debugMode is true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.However, if throwException is true, CvException is thrown instead of calling Debug.LogError (msg).
        * <br>Please use as follows.
        * <br>Utils.setDebugMode(true);
        * <br>aaa
        * <br>bbb
        * <br>ccc
        * <br>Utils.setDebugMode(false);
        * 
        * @param debugMode if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
        * @param throwException if true, CvException is thrown instead of calling Debug.LogError (msg).
        * @param callback callback called when an OpenCV error occurs on the Native side.
        */
        public static void setDebugMode(bool debugMode, bool throwException = false, Action<string> callback = null)
        {
            DlibFaceLandmarkDetector_SetDebugMode(debugMode);

            if (debugMode)
            {
                DlibFaceLandmarkDetector_SetDebugLogFunc(debugLogFunc);
                //DlibFaceLandmarkDetector_DebugLogTest ();

                throwDlibException = throwException;
                dlibSetDebugModeCallback = callback;
            }
            else
            {
                DlibFaceLandmarkDetector_SetDebugLogFunc(null);

                throwDlibException = false;
                dlibSetDebugModeCallback = null;
            }
        }

        private delegate void DebugLogDelegate(string str);

        [MonoPInvokeCallback(typeof(DebugLogDelegate))]
        private static void debugLogFunc(string str)
        {
            if (dlibSetDebugModeCallback != null) dlibSetDebugModeCallback.Invoke(str);

            if (throwDlibException)
            {
#if UNITY_2022_2_OR_NEWER && UNITY_ANDROID && ENABLE_IL2CPP
                Debug.LogError(str);
#else
                throw new DlibException(str);
#endif
            }
            else
            {
                Debug.LogError(str);
            }
        }

#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
#else
        const string LIBNAME = "dlibfacelandmarkdetector";
#endif

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_SetDebugMode([MarshalAs(UnmanagedType.U1)] bool flag);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_SetDebugLogFunc(DebugLogDelegate func);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_DebugLogTest();
    }
}