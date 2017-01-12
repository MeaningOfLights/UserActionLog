using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;

namespace JeremyThompsonLabs.UserActionLog.TestWinFormApp
{
    public partial class Form1 : Form
    {
        private UserActions _userActions;

        public Form1()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            InitializeComponent();

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Col1");
            dt.Columns.Add("Col2");
            var dr = dt.NewRow(); dr[0] = "Abra"; dr[1] = "Cadabra";dt.Rows.Add(dr);
            dr = dt.NewRow(); dr[0] = "Click"; dr[1] = "All"; dt.Rows.Add(dr);
            dr = dt.NewRow(); dr[0] = "the"; dr[1] = "controls"; dt.Rows.Add(dr);
            dataGridView1.DataSource = dt;
        }

        /// <summary>
        /// Wire up the User Action Logger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            _userActions = new UserActionLog.UserActions(this);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _userActions.FinishLoggingUserActions(this);
        }
        
        /// <summary>
        /// Cause an exception (division by zero) to cause the Users Actions leading up to the exception to be emailed to support
        /// </summary>
        /// <remarks>
        /// The trick is in the stacktrace, the last call in the stack will tell you the last user action.  The Log of Actions tells you how to get the program 
        /// in the state before the unhandled exception occurred. Once you get it to that point, follow the action in the StackTrace to fault the application.
        /// </remarks>
        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            int j = 1;
            int k = j / i;
        }
        
        private void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            EmailExceptionAndActionLogToSupport(sender, t.Exception);
        }
        
        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            EmailExceptionAndActionLogToSupport(sender,(Exception)e.ExceptionObject);
        }
        
        #region "Email Support unhandled exceptions"

        StringBuilder _errMsg = new StringBuilder();
        List<string> _emailAttachments = new List<string>();
        
        private void EmailExceptionAndActionLogToSupport(object sender, Exception exception)
        {
            //HTML table containing the Exception details for support email
            _errMsg.Append("<table><tr><td colSpan=1><b>User:</b></td><td colSpan=2>");
            _errMsg.Append(Environment.UserName);
            _errMsg.Append("</td></tr>");
            _errMsg.Append("<tr><td><b>Time:</b></td><td>");
            _errMsg.Append(new DateTime().ToShortTimeString());
            _errMsg.Append("</td></tr><tr></tr>");
            _errMsg.Append("<tr><td><b>Exception Type:</b></td><td>");
            _errMsg.Append(sender.ToString() + " Exception");
            _errMsg.Append("</td></tr><tr></tr>");

            if ((exception) != null)
            {
                _errMsg.Append("<tr><td><b>Message:</b></td><td>");
                _errMsg.Append(exception.Message.Replace(" at ", " at <br>"));
                _errMsg.Append("</td></tr><tr></tr>");
                if (exception.InnerException != null)
                {
                    _errMsg.Append("<tr><td><b>Inner Exception:</b></td><td>");
                    _errMsg.Append(exception.InnerException.Message);
                    _errMsg.Append("</td></tr>");
                }
                _errMsg.Append("<tr><td><b>Stacktrace:</b></td><td>");
                _errMsg.Append(exception.StackTrace);
                _errMsg.Append("</td></tr></table>");
            }

            //Write out the logs in memory to file
            _userActions.FinishLoggingUserActions(this);

            //Get list of today's log files
            _emailAttachments.AddRange(_userActions.GetTodaysLogFileNames());

            //Adding a screenshot of the broken window for support is a good touch
            _emailAttachments.Add(Screenshot.TakeScreenshotReturnFilePath());

            EWSEmail emailSystem = new EWSEmail();
            emailSystem.SendEmail(ConfigurationManager.AppSettings["EmailSupport"].ToString(), "", "", "PROJECT_NAME - PROBLEM CASE ID: " + Path.GetRandomFileName(), _errMsg.ToString(), _emailAttachments.ToArray());

        }

        #endregion

    }
}
