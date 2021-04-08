﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lanchat.Core.API
{
    internal class JsonBuffer
    {
        private string buffer;
        private string currentJson;
        
        internal void AddToBuffer(string dataString)
        {
            buffer += dataString;
        }

        internal List<string> ReadBuffer()
        {
            var index = buffer.LastIndexOf("}", StringComparison.Ordinal);
            if (index < 0) return new List<string>();
            currentJson = buffer.Substring(0, index + 1);
            buffer = buffer.Substring(index + 1);
            return currentJson.Replace("}{", "}|{").Split('|').ToList();
        }
    }
}