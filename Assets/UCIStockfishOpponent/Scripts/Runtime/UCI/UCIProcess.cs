using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ChessEngine.UCIStockfishOpponent.Threading;

namespace ChessEngine.UCIStockfishOpponent.UCI
{
    /// <summary>
    /// A component that starts and manages a UCI (Universal Chess Interface) process.
    /// 
    /// NOTE: This process is responsible for getting the UCI ready and providing a low level interface, it does not actually provide methods for playing through a game -- use a UCIGameManager for this.
    /// NOTE: bestmove commands cab have values of 4 or 5 characters (e.g: e2e4, b7b8q) the suffix 'q' denoting that the move was 'queened'.
    /// </summary>
    /// Author: Intuitive Gaming Solutions
    public class UCIProcess : MonoBehaviour
    {
        [Header("Settings - UCI")]
        [Tooltip("The path to the UCI process. First the direct path will be checked for the process, then the Application.dataPath will be checked via the relative path.")]
        public string processPath;
        [Tooltip("The number of seconds the process will wait for 'uciok' to be received before terminating.")]
        public float uciTimeout = 10f;
        [Tooltip("Should UCI inputs be logged?")]
        public bool enableInputLogging;
        [Tooltip("Should UCI outputs be logged?")]
        public bool enableOutputLogging;

        [Header("Events - UCI")]
        [Tooltip("An event that is invoked whenever input is send to the UCI process.\n\nArg0: string - The input that was sent to the process.")]
        public UCIUnityEvent UCIInputSent;
        [Tooltip("An event that is invoked whenever output is received from the UCI process.\n\nArg0: string - The output that was received from the process.")]
        public UCIUnityEvent UCIOutputReceived;
        [Tooltip("An event that is invoked whenever a 'bestmove' is received from the UCI process.\n\nArg0: string - 4 characters describing the move that was received. (e.g: e2e4)")]
        public UCIUnityEvent UCIBestMoveReceived;
        [Tooltip("An Action that is invoked whenever 'uciok' is received from the UCI process.")]
        public UnityEvent UCIOkReceived;
        [Tooltip("An Action that is invoked whenever an expected 'uciok' command is not received from the UCI engine within uciTimeout seconds.")]
        public UnityEvent UCIOkTimeout;
        [Tooltip("An Action that is invoked whenever 'readyok' is received from the UCI process.")]
        public UnityEvent UCIReadyReceived;
        [Tooltip("An Action that is invoked just before the UCI process is stopped.")]
        public UnityEvent UCIProcessStopped;

        /// <summary>The Process that was started.</summary>
        public Process Process { get; protected set; }
        /// <summary>Returns true if the UCI process is running, otherwise false.</summary>
        public bool IsProcessRunning
        {
            get
            {
                // No process, not running.
                if (Process == null)
                    return false;

                try
                {
                    Process.GetProcessById(Process.Id);
                    return true;
                }
                catch { return false; }
            }
        }
        /// <summary>Returns true if 'uciok' was received from the UCI process since the last time uci was initialized, otherwise false.</summary>
        public bool IsUCIOk { get; protected set; }
        /// <summary>Returns the number of valid UCI options for this process.</summary>
        public float UCIOptionsCount { get { return m_UCIOptions.Count; } }

        /// <summary>Returns true if the process was started, otherwise false. (Does not check if the process is still running or not.)</summary>
        protected bool m_IsProcessStarted;
        /// <summary>The Time.realtimeSinceStartup the 'uciok' command is expected no later than, or float.NegativeInfinity if not expected.</summary>
        protected float m_ExpectedUCIOkTime = float.NegativeInfinity;

        /// <summary>A list of UCI options available for the current UCI process.</summary>
        protected List<UCIOption> m_UCIOptions;

        /// <summary>A MainThreadDispathcher used to dispatch events received on the non-main thread on the main thread.</summary>
        protected MainThreadDispatcher m_MainThreadDispatcher;

        // Unity callback(s).
        void Awake()
        {
            // Instantiate a main thread dispatcher.
            m_MainThreadDispatcher = new MainThreadDispatcher();
        }

        void Start()
        {
            // Subscribe to the main thread dispatcher's event(s).
            m_MainThreadDispatcher.EventDispatched += OnMainThreadEventDispatched;
        }

        void OnDestroy()
        {
            // Clean up main thread dispatcher if non null.
            if (m_MainThreadDispatcher != null)
            {
                // Unsubscribe from main thread dispatcher event(s).
                m_MainThreadDispatcher.EventDispatched -= OnMainThreadEventDispatched;
            }
        }

        void Update()
        {
            // Process the main thread dispatcher queue.
            if (m_MainThreadDispatcher != null)
                m_MainThreadDispatcher.ProcessQueue();

            // Check if 'uciok' wait time has expired.
            if (!IsUCIOk && m_ExpectedUCIOkTime != float.NegativeInfinity && Time.time > m_ExpectedUCIOkTime)
            {
                // Timed out.
                OnUCIOkTimeout();

                // Time expired! timeout.
                StopProcess();
            }
        }

        // Public method(s).
        /// <summary>Starts the UCI process. Logs a warning on failure.</summary>
        public void StartProcess()
        {
            if (!TryStartProcess())
                UnityEngine.Debug.LogWarning("Failed to start UCI process!", gameObject);
        }

        /// <summary>Tries to start the UCI process returning the result.</summary>
        /// <returns>true if the process started successfully, otherwise false.</returns>
        public bool TryStartProcess()
        {
            // UCI not ok.
            IsUCIOk = false;

            // Look for process.
            string realPath = GetProcessPath();
            if (realPath != null)
            {
                // Create the start info for the process.
                ProcessStartInfo startProcessInfo = new ProcessStartInfo()
                {
                    FileName = realPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };

                // Create the process object.
                Process = new Process();
                Process.StartInfo = startProcessInfo;
                try
                {
                    // Throws an exception on windows 98.
                    Process.PriorityClass = ProcessPriorityClass.BelowNormal;
                }
                catch { }
                Process.OutputDataReceived += new DataReceivedEventHandler(Thread_OnOutputDataReceived);

                // Start the process.
                if (Process.Start())
                {
                    Process.BeginErrorReadLine();
                    Process.BeginOutputReadLine();
                    m_IsProcessStarted = true;

                    // Initialize the UCI.
                    InitializeUCI();
                    
                    // Process started.
                    return true;
                }
            }
            else
            {
                // Process not found, log error.
                UnityEngine.Debug.LogError("Process not found with direct or relative path of '" + processPath + "'! Make sure you either specify the direct path or the executable for the UCI is in the Application.dataPath directory.", gameObject);
            }

            // Process not started.
            return false;
        }

        /// <summary>Stops the UCI process.</summary>
        public void StopProcess()
        {
            // UCI is not ok.
            IsUCIOk = false;

            // Invoke the 'UCIProcessStopped' event.
            UCIProcessStopped?.Invoke();

            // Close the process if valid.
            if (Process != null)
            {
                Process.Close();
                Process = null;
            }
        }

        /// <summary>Sends the given input, pInput, to the process' StandardInput.</summary>
        /// <param name="pInput"></param>
        public void SendInput(string pInput)
        {
            // If input logging is enabled log UCI output.
            if (enableInputLogging)
                UnityEngine.Debug.Log("[input] " + pInput);

            // Sends the standard input.
            Process.StandardInput.WriteLine(pInput);
            Process.StandardInput.Flush();

            // Invoke the 'input sent' Unity event.
            UCIInputSent?.Invoke(pInput);
        }

        /// <summary>Looks for the processPath executable in various locations, returns a string representing the full path if found, otherwise null.</summary>
        /// <returns>A string representing the real, full process path or null.</returns>
        public string GetProcessPath()
        {
            // Look for the process.
            if (!File.Exists(processPath))
            {
                // Test all valid paths.
                string testPath = Path.Combine(Application.dataPath, processPath);

                // Check in Application.dataPath/processPath.
                if (File.Exists(testPath))
                    return testPath;
            }
            else { return processPath; }

            // Not found, return null.
            return null;
        }

        /// <summary>
        /// Returns the UCIOption at the given index, pIndex.
        /// This method does not perform bounds checking and assumes pIndex is within the bounds of the UCI options array. Use 'UCIOptionsCount' to determine the number of valid UCI options.
        /// </summary>
        /// <param name="pIndex"></param>
        /// <returns>the UCIOption at the given index</returns>
        public UCIOption GetUCIOption(int pIndex) { return m_UCIOptions[pIndex]; }

        /// <summary>Sets the 'enableInputLogging' field of this component. Useful for use with Unity editor events.</summary>
        /// <param name="pEnabled"></param>
        public void SetInputLoggingEnabled(bool pEnabled) { enableInputLogging = pEnabled; }
        /// <summary>Sets the 'enableOutputLogging' field of this component. Useful for use with Unity editor events.</summary>
        /// <param name="pEnabled"></param>
        public void SetOutputLoggingEnabled(bool pEnabled) { enableOutputLogging = pEnabled; }

        // Private method(s).
        /// <summary>Initializes the UCI by sending the 'uci' command and waiting for a response, 'uciok'.</summary>
        void InitializeUCI()
        {
            // Send 'uci' input.
            SendInput("uci");

            // UCI ok pending.
            IsUCIOk = false;

            // Allocate UCI options list.
            m_UCIOptions = new List<UCIOption>();

            // Set expected UCI response time.
            m_ExpectedUCIOkTime = Time.realtimeSinceStartup + uciTimeout;
        }

        // Private callback(s).
        /// <summary>Invoked whenever the 'uciok' command is received while the UCI is not already 'ok'.</summary>
        void OnUCIOk()
        {
            // UCI is ok.
            IsUCIOk = true;

            // No expected UCI response time.
            m_ExpectedUCIOkTime = float.NegativeInfinity;

            // Invoke the 'UCIOkReceived' event.
            UCIOkReceived?.Invoke();
        }

        /// <summary>Invoked when an expected 'uciok' command is not received within ucpTimeout seconds after initializing the UCI.</summary>
        void OnUCIOkTimeout()
        {
            // Invoke the 'UCIOkTimeout' event.
            UCIOkTimeout?.Invoke();
        }

        /// <summary>
        /// Invoked whenever data is received from the uci process.
        /// WARNING: This method is not invoked on the main thread.
        /// </summary>
        /// <param name="pSender"></param>
        /// <param name="pEventArgs"></param>
        void Thread_OnOutputDataReceived(object pSender, DataReceivedEventArgs pEventArgs)
        {
            // Add the data to the main thread dispatcher.
            m_MainThreadDispatcher.Enqueue(pEventArgs.Data);
        }

        void OnMainThreadEventDispatched(string pData)
        {
            // If output logging is enabled log UCI output.
            if (enableOutputLogging)
                UnityEngine.Debug.Log("[output] " + pData);

            // Split command into parts.
            string[] inputParts = pData.Split(' ');

            // Handle UCI commands.
            string command = inputParts[0].Trim().ToLower();
            if (command == "bestmove")
            {
                if (inputParts.Length > 1)
                {
                    // Move received.
                    string move = inputParts[1].Trim().ToLower();
                    if (move.Length == 4 || move.Length == 5)
                    {
                        // Ensure the move is 2 moves in the specified format (a to h)(1 to 8).
                        if (InputValidation.IsCharacterValidColumn(move[0]) && InputValidation.IsCharacterValidRank(move[1]) && InputValidation.IsCharacterValidColumn(move[2]) && InputValidation.IsCharacterValidRank(move[3]))
                        {
                            // Invoke the 'UCIBestMoveReceived' Unity event.
                            UCIBestMoveReceived?.Invoke(move);
                        }
                        else { UnityEngine.Debug.LogWarning("Malformed 'bestmove' received from UCIProcess! The 'move' is exepcted in the format 'bestmove <a-h><1-8><a-h><1-8><(Optional)q>' (e.g: bestmove e2e4). The 'move' was given as '" + move + "'.", gameObject); }
                    }
                    else { UnityEngine.Debug.LogWarning("Malformed 'bestmove' received from UCIProcess! The 'move' is expected in 4 or 5 characters (e.g: e2e4, b7b8q). The 'move' was given as '" + move + "'.", gameObject); }
                }
                else { UnityEngine.Debug.LogWarning("Malformed 'bestmove' received from UCIProcess! No move specified.", gameObject); }
            }
            else if (command == "option")
            {
                // Received UCI option.
                Regex optionNameRegex = new Regex("name\\s(.*)\\stype", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection optionNameMatches = optionNameRegex.Matches(pData);
                if (optionNameMatches.Count > 0)
                {
                    // Get UCI option name.
                    string uciOptionName = optionNameMatches[0].Groups[1].Value.Trim();

                    // Get UCI option type.
                    Regex optionTypeRegex = new Regex("type\\s+(.*?)(?:default|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection optionTypeMatches = optionTypeRegex.Matches(pData);
                    if (optionTypeMatches.Count > 0)
                    {
                        string uciOptionType = optionTypeMatches[0].Groups[1].Value.Trim();

                        // Find UCI option default value (if there is one).
                        string uciOptionDefaultValue = null;
                        Regex optionDefaultValueRegex = new Regex("default\\s(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        MatchCollection optionDefaultValueMatches = optionDefaultValueRegex.Matches(pData);
                        if (optionDefaultValueMatches.Count > 0)
                        {
                            // Set default value to specified.
                            uciOptionDefaultValue = optionDefaultValueMatches[0].Groups[1].Value.Trim();
                        }

                        // Ensure it is not a duplicate option before adding it.
                        int optionIndex = -1;
                        for (int i = 0; i < m_UCIOptions.Count; ++i)
                        {
                            // Check for name match.
                            if (m_UCIOptions[i].name == uciOptionName)
                            {
                                // Match found!
                                optionIndex = i;
                                break;
                            }
                        }

                        // If no match was found register the UCI option.
                        if (optionIndex == -1)
                        {
                            m_UCIOptions.Add(new UCIOption()
                            {
                                name = uciOptionName,
                                type = uciOptionType,
                                defaultValue = uciOptionDefaultValue
                            });
                        }
                        // Since a match was found do not re-register the UCI option, only modify the type and default value if one was specified...
                        else
                        {
                            m_UCIOptions[optionIndex].type = uciOptionType;
                            m_UCIOptions[optionIndex].defaultValue = uciOptionDefaultValue;
                        }
                    }
                    else { UnityEngine.Debug.LogWarning("Unable to find 'type' for UCI option given by output line '" + pData + "'!", gameObject); }
                }
                else { UnityEngine.Debug.LogWarning("Unable to find 'name' for UCI option given by output line '" + pData + "'!", gameObject); }
            }
            else if (command == "uciok")
            {
                // Log warning if uciok was unexpectedly received.
                if (!IsUCIOk)
                {
                    // UCI is ok.
                    OnUCIOk();
                }
                else { UnityEngine.Debug.LogWarning("Received unexpected 'uciok' over UCI from the chess engine.", gameObject); }
            }
            else if (command == "readyok")
            {
                // Invoke the 'UCI ReadyReceived' event.
                UCIReadyReceived?.Invoke();
            }

            // Invoke the 'output received' Unity event.
            UCIOutputReceived?.Invoke(pData);
        }
    }
}
