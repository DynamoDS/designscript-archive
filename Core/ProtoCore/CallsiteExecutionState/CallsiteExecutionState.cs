
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using ProtoCore.Utils;
using System.Xml.Serialization;
using System.Text;

namespace ProtoCore
{
    public static class TLSUtils
    {
        public static string GetTLSData()
        {
             return "hello world data";
        }
    }

    public class CallsiteExecutionState
    {

#region Static_Utils

        private static string ext = "csstate";
        public static string filename = "vmstate_test";

        private static FileStream fileStream = null;
        private static string filePath = GetThisSessionFileName();

        /// <summary>
        /// Generate a callsite guid, given a functiongroup ID and the expression ID
        /// </summary>
        /// <param name="functionUID"></param>
        /// <param name="ExprUID"></param>
        /// <returns></returns>
        public static string GenerateCallsiteGUID(string functionUID, int ExprUID)
        {
            // This is a naive implementation, explore a better one
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            string inString = functionUID + ExprUID.ToString();
            
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(inString);
            byte[] hash = md5.ComputeHash(inputBytes);


            // Convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }


        public static string GetThisSessionFileName()
        {
            return filename + "." + ext;
        }

#endregion

#region Static_LoadStore_Methods

        public static bool SaveState(CallsiteExecutionState data)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CallsiteExecutionState));
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(fileStream, data);
                fileStream.Close();
            }
            catch (Exception e)
            {
                return false;
            }

            if (null != fileStream)
            {
                fileStream.Close();
            }
            return true;
        }

        public static CallsiteExecutionState LoadState()
        {
            CallsiteExecutionState csState = null;
            if (!string.IsNullOrEmpty(filePath) && (File.Exists(filePath)))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CallsiteExecutionState));
                    fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    csState = serializer.Deserialize(fileStream) as CallsiteExecutionState;
                }
                catch (Exception)
                {
                    csState = new CallsiteExecutionState();
                }
            }
            else
            {
                csState = new CallsiteExecutionState();
            }

            return csState;
        }

#endregion


        public CallsiteExecutionState()
        {
            //CallsiteDataMap = new Dictionary<string, string>();
            key = new List<string>();
            value = new List<string>();
        }

        /// <summary>
        /// CallsiteDataMap is the mapping between a unique callsite ID and the associated data
        /// This data is updated for every call to a callsite
        /// Determine why this needs to be public in order to serialize
        /// </summary>
        //public Dictionary<string, string> CallsiteDataMap { get; set; }

        public List<string> key { get; set; }
        public List<string> value { get; set; }


        //public void Store(string callsiteID, string data)
        //{
        //    Validity.Assert(null != CallsiteDataMap);
        //    if (CallsiteDataMap.ContainsKey(callsiteID))
        //    {
        //        CallsiteDataMap[callsiteID] = data;
        //    }
        //    else
        //    {
        //        CallsiteDataMap.Add(callsiteID, data);
        //    }
        //}

        //public string Load(string callsiteID)
        //{
        //    Validity.Assert(null != CallsiteDataMap);
        //    if (CallsiteDataMap.ContainsKey(callsiteID))
        //    {
        //        return CallsiteDataMap[callsiteID];
        //    }
        //    return null;
        //}

        public void Store(string callsiteID, Object dataObj)
        {
            // TODO Jun: implement serializable object
            string data = dataObj as string;

            int valueIndex = -1;
            if (key.Contains(callsiteID))
            {
                valueIndex = key.IndexOf(callsiteID);
                value[valueIndex] = data;
            }
            else
            {
                key.Add(callsiteID);
                value.Add(data);
            }
        }

        public string Load(string callsiteID)
        {
            int valueIndex = -1;
            if (key.Contains(callsiteID))
            {
                valueIndex = key.IndexOf(callsiteID);
                return value[valueIndex];
            }
            return null;
        }

        public int GetVMStateCount()
        {
            Validity.Assert(null != key);
            return key.Count;
        }

        /// <summary>
        /// This method builds a dictionary given the Lsit of keys and values
        /// This will be obsolete if once the serializable dictionary is implemented
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> BuildCSStateDictionary()
        {
            Validity.Assert(null != key);
            Validity.Assert(null != value);
            Validity.Assert(key.Count == value.Count);

            Dictionary<string, string> vmstate = new Dictionary<string, string>();
            int index = 0;
            foreach(string id in key)
            {
                vmstate.Add(id, value[index++]);
            }
            return vmstate;
        }
    }
}
