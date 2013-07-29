/**
 * 
 * Luke Church, 2011
 * luke@church.name
 * 
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;

namespace net.riversofdata.dhlogger
{
	public class Log : IDisposable
	{
        private const string URL = "https://dataharvestapp.appspot.com/rpc";
        private const int MAX_DATA_LENGTH = 750000;
        private const int MAX_NAME_LENGTH = 256;
        private const String AppFolderName = "dh-logLib-Cache";
        private const String dbFileName = "UsabilityLog.db";
        private const String tableName = "LOGITEMS";
        private SQLiteConnection conn;

        /// <summary>
        /// 
        /// This mutex guards transactions on the DB
        /// 
        /// This is not actually necessary as SQLLite should enforce ACID,
        /// but rather another defensive programming mechanism
        /// </summary>
        private object dbMutex = new object();


        #region State holders

        private string appName;
		/// <summary>
		/// The name of the application associated with the log entries
		/// </summary>
        public string AppName {
            get
            {
                return appName;
            }
			set
            {
                if (!ValidateTextContent(value))
                    throw new ArgumentException(
                        "App name must only be letters, numbers or -");

                if (!ValidateLength(value))
                    throw new ArgumentException("App Name must be 256 chars or less");


                appName = value;
            }
		}

        private string userID;
		/// <summary>
		/// A Guid string for the user associated with the log entries
		/// </summary>
        public string UserID {
            get
            {
                return userID;
            }
            set
            {


                if (!ValidateTextContent(value))
                    throw new ArgumentException(
                        "User ID name must only be letters, numbers or -");

                if (!ValidateLength(value))
                    throw new ArgumentException("UserID must be 256 chars or less");


                userID = value;
            }
		}
        private string sessionID;
        /// <summary>
        /// A Guid string for the session that the user is engaging with
        /// </summary>
		public string SessionID {
            get
            {
                return sessionID;
            }
            set
            {
                if (!ValidateTextContent(value))
                    throw new ArgumentException(
                        "Session ID name must only be letters, numbers or -");

                if (!ValidateLength(value))
                    throw new ArgumentException("Session ID must be 256 chars or less");


                sessionID = value;
            }
        }


        #endregion

        /// <summary>
        /// An object used to guard the uploadedItemsCount
        /// </summary>
        private Object uploadedItemsMutex = new object();
        private long uploadedItemsCount = 0;
        
        /// <summary>
        /// The number of items that this log entity has successfully uploaded
        /// This may be greater than the number of calls due to splitting of large entries
        /// </summary>
        public long UploadedItems
        {
            get
            {
                lock (uploadedItemsMutex)
                {
                    return uploadedItemsCount;
                }
            }


            private set
            {
                lock (uploadedItemsMutex)
                {
                    uploadedItemsCount = value;
                }
            }

            
        }
		
		
        
        System.Diagnostics.Stopwatch sw;
		
		private Thread uploaderThread;
		private Queue<Dictionary<String, String>> items;
		private const int DELAY_MS = 1000;

        #region Public API


        /// <summary>
        /// Create a new log instance
        /// </summary>
        /// <param name="appName">The name of the application associated with this log</param>
        /// <param name="userID">A statistically unique string associated with the user, e.g. a GUID</param>
        /// <param name="sessionID">A statistically unique string associated with the session, e.g. a GUID</param>
		public Log (string appName, string userID, string sessionID)
		{
            string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullFolderName = dataFolder + "\\" + AppFolderName;

            if (!Directory.Exists(fullFolderName))
                Directory.CreateDirectory(fullFolderName);


            string connString = "Data Source=" + fullFolderName + "\\" + dbFileName;


            conn = new System.Data.SQLite.SQLiteConnection(connString);
            conn.Open();


            //See if we already have the table, if not, create it

            lock (dbMutex)
            {
                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE name='" + tableName + "'";


                SQLiteDataReader rdr = cmd.ExecuteReader();

                if (!rdr.HasRows)
                {
                    SQLiteCommand createTable = new SQLiteCommand(conn);
                    createTable.CommandText
                        = "CREATE TABLE '" + tableName + @"' (
'EntryID' String,
'AppName' String,
'UserID' String,
'SessionID' String,
'Tag' String,
'Priority' String,
'Text' String,
'DateTime' String,
'MicroTime' String
);";

                    System.Diagnostics.Debug.WriteLine("About to Exec: " + createTable.CommandText);

                    SQLiteTransaction trans = conn.BeginTransaction();
                    createTable.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            
			AppName = appName;
			UserID = userID;
			SessionID = sessionID;
			
			this.sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			
			items = new Queue<Dictionary<string, string>>();
			
			uploaderThread = new Thread(new ThreadStart(UploaderExec));
			uploaderThread.IsBackground = true;
			uploaderThread.Start();
		}


        /// <summary>
        /// Log an item at debug priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
		public void Debug(string tag, string text)
        {
            ValidateInput(tag, text);
            PrepAndPushItem(tag, "Debug", text);
		}


        /// <summary>
        /// Log an item at error priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
		public void Error(string tag, string text)
		{
            ValidateInput(tag, text);
			PrepAndPushItem(tag, "Error", text);
		}


        /// <summary>
        /// Log an item at info priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
		public void Info(string tag, string text)
		{
            ValidateInput(tag, text);
			PrepAndPushItem(tag, "Info", text);
		}

        /// <summary>
        /// Log an item at verbose priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
		public void Verbose(string tag, string text)
		{
            ValidateInput(tag, text);
			PrepAndPushItem(tag, "Verbose", text);
		}
		
		#endregion


		/// <summary>
		/// Methods that preps a write request
        /// This method calls recursively up to once if the text is too large and needs
        /// splitting
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="priority"></param>
		/// <param name="text"></param>
		private void PrepAndPushItem(string tag, string priority, string text)
		{
            //We don't need to validate the content of text as it's going to get base64
            //encoded
            System.Diagnostics.Debug.Assert(ValidateTextContent(tag));
            System.Diagnostics.Debug.Assert(ValidateTextContent(priority));

            List<String> splitText = SplitText(text);

            if (splitText.Count > 1)
            {
                //Add a GUID and a serial to the Tag (We've reserved enough space for this)
                //Recursive call the write with the split text
                Guid g = Guid.NewGuid();

                for (int i = 0; i < splitText.Count; i++)
                {
                    PrepAndPushItem(
                        tag + "-" + g.ToString() + "-" + i.ToString(),
                        priority,
                        splitText[i]);
                }

                return;
            }

            text = splitText[0];
			
			byte[] byteRepresentation = System.Text.Encoding.UTF8.GetBytes(text);
			string safeStr = System.Convert.ToBase64String(byteRepresentation);


			//Destroy the original representations to ensure runtime errors if used later in this method
			text = null;
			
			
			string dateTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
			string microTime = sw.ElapsedMilliseconds.ToString();
			
			Dictionary<String, String> item = new Dictionary<String, String>();
			
			item.Add("Tag", tag);
			item.Add("Priority", priority);
			item.Add("AppIdent", AppName);
			item.Add("UserID", UserID);
			item.Add("SessionID", SessionID);
			item.Add("DateTime", dateTime);
			item.Add("MicroTime", microTime);
			
			item.Add("Data", safeStr);
			
			PushItem(item);
			
		}
		
		/// <summary>
		/// ASync add the log item onto the queue to be pushed to the db
		/// </summary>
		/// <param name="item">
		/// A <see cref="Dictionary<String, String>"/>
		/// </param>
		private void PushItem(Dictionary<String, String> item)
		{
            SQLiteCommand insertItem = new SQLiteCommand(conn);
            insertItem.CommandText
                = "INSERT INTO " + tableName + 
@" (EntryID, AppName, UserID, SessionID, Tag, Priority, Text, DateTime, MicroTime)
VALUES ('" + Guid.NewGuid() + "', '" +
          item["AppIdent"] + "', '" +
          item["UserID"] + "', '" +
          item["SessionID"] + "', '" +
          item["Tag"] + "', '" +
          item["Priority"] + "', '" +
          item["Data"] + "', '" +
          item["DateTime"] + "', '" +
          item["MicroTime"] + "')";

            System.Diagnostics.Debug.WriteLine("About to Exec: " + insertItem.CommandText);

            lock (dbMutex)
            {

                SQLiteTransaction trans = conn.BeginTransaction();
                insertItem.ExecuteNonQuery();
                trans.Commit();
            }

        }

        #region Uploader Thread methods

        /// <summary>
        /// Core thread method for handling the upload
        /// </summary>
		private void UploaderExec()
		{
			while (true)
			{
				Thread.Sleep(DELAY_MS);
				

                //Pull a list of the entries from the db
                //We don't need a transaction for this, it's only a read at this point

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "SELECT * FROM " + tableName;

                System.Diagnostics.Debug.WriteLine("About to exec: " + cmd.CommandText);

                List<Dictionary<String, String>> items = new List<Dictionary<string, string>>();

                lock (dbMutex)
                {
                    SQLiteDataReader rdr = cmd.ExecuteReader();


                    while (rdr.Read())
                    {
                        //Build the dictionary back from the DB
                        Dictionary<String, String> item = new Dictionary<String, String>();


                        //Table names
                        //(EntryID, AppName, UserID, SessionID, Tag, Priority, Text, DateTime, MicroTime)

                        item.Add("EntryID", (string)rdr["EntryID"]);
                        item.Add("Tag", (string)rdr["Tag"]);
                        item.Add("Priority", (string)rdr["Priority"]);
                        item.Add("AppIdent", (string)rdr["AppName"]);
                        item.Add("UserID", (string)rdr["UserID"]);
                        item.Add("SessionID", (string)rdr["SessionID"]);
                        item.Add("DateTime", (string)rdr["DateTime"]);
                        item.Add("MicroTime", (string)rdr["MicroTime"]);
                        item.Add("Data", (string)rdr["Text"]);

                        items.Add(item);
                    }

                }

                List<Dictionary<String, String>> failedItems = new List<Dictionary<String, String>>();

                foreach (Dictionary<String, String> item in items)
                    if (!UploadItem(item))
                        failedItems.Add(item);

                //Now delete all the items other than the failed ones

                List<String> IDsToDelete = new List<string>();

                foreach (Dictionary<String, String> item in items)
                    if (!failedItems.Contains(item))
                        IDsToDelete.Add(item["EntryID"]);

                if (IDsToDelete.Count > 0)
                {
                    
                    SQLiteCommand deleteItems = new SQLiteCommand(conn);
                    deleteItems.CommandText
                        = "DELETE FROM " + tableName + " WHERE EntryID IN (";

                    for (int i = 0; i < IDsToDelete.Count; i++)
                    {
                        deleteItems.CommandText += "'" + IDsToDelete[i] + "'";

                        if (i < IDsToDelete.Count - 1)
                            deleteItems.CommandText += ",";
                    }

                    deleteItems.CommandText += ")";


                    System.Diagnostics.Debug.WriteLine("About to Exec: " + deleteItems.CommandText);

                    lock (dbMutex)
                    {
                        SQLiteTransaction trans = conn.BeginTransaction();
                        deleteItems.ExecuteNonQuery();
                        trans.Commit();
                    }

                }


			}
			
		}
		
        /// <summary>
        /// Code to transfer an item iver the network
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if success, false otherwise</returns>
		private bool UploadItem(Dictionary<String, String> item)
		{
			try
			{
				
			    StringBuilder sb = new StringBuilder();
				sb.Append("[\"BasicStore\", {");
				
				bool first = true;
				
				foreach (string key in item.Keys)
				{
					if (!first)
						sb.Append(",");
					else
						first = false;
					
					sb.Append("\"");
					sb.Append(key);
					sb.Append("\" : \"");
					sb.Append(item[key]);
					sb.Append("\"");
					
				}
				
				sb.Append("}]");
				
				System.Diagnostics.Debug.WriteLine(sb.ToString());
				
				 WebRequest request = WebRequest.Create (URL);
            
			request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes (sb.ToString());
			
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream ();
            dataStream.Write (byteArray, 0, byteArray.Length);
            dataStream.Close ();
            WebResponse response = request.GetResponse ();
            
				if (((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
					throw new Exception(((HttpWebResponse)response).StatusDescription);
				
            dataStream = response.GetResponseStream ();
            StreamReader reader = new StreamReader (dataStream);
            string responseFromServer = reader.ReadToEnd ();
            System.Diagnostics.Debug.WriteLine (responseFromServer);

            
			reader.Close ();
            dataStream.Close ();
            response.Close ();


            UploadedItems += long.Parse(responseFromServer);

            	
				
				return true;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
				return false;
				
			}



        }

        #endregion




        #region Helper methods

        /// <summary>
        /// Split a string into shorter strings as defined by the const
        /// params
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<String> SplitText(String text)
        {
            int len = text.Length;

            List<String> ret = new List<string>();
            int added = 0;
            for (int i = 0; i < len / (MAX_DATA_LENGTH+1); i++)
            {
                ret.Add(text.Substring(i*MAX_DATA_LENGTH, MAX_DATA_LENGTH));
                added += MAX_DATA_LENGTH;
            }

            ret.Add(text.Substring(added));

#if DEBUG
            int totalTextCount = 0;
            foreach (String str in ret)
                totalTextCount += str.Length;

            System.Diagnostics.Debug.Assert(totalTextCount == len);
#endif

            return ret;
        }

        /// <summary>
        /// Do input validation tests
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="text"></param>
        private void ValidateInput(string tag, string text)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag must not be null");

            if (text == null)
                throw new ArgumentNullException("Text must not be null");

            if (!ValidateLength(tag))
                throw new ArgumentException("Tag must be 256 chars or less");


            if (!ValidateTextContent(tag))
                throw new ArgumentException(
                    "Tag must only be letters, numbers or '-', '.'");

        }

        /// <summary>
        /// Ensure that the text that is being sent is only alphanumeric
        /// and hypen
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool ValidateTextContent(string str)
        {
            char[] chars = str.ToCharArray();

            foreach (char ch in chars)
            {
                if (!(char.IsLetterOrDigit(ch) || ch == '-' || ch=='.'))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure that the names and tags fit within the safe window after
        /// we've put a GUID and a serial on the end of them
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool ValidateLength(string str)
        {
            if (str == null)
                return false;

            if (str.Length > MAX_NAME_LENGTH)
                return false;
            else
                return true;

        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                uploaderThread.Abort();

                conn.Close();
                conn.Dispose();
            }
            catch (Exception)
            { 
                
            }
        }

        #endregion
    }
}

