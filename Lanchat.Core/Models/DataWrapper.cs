﻿namespace Lanchat.Core.Models
{
    public class DataWrapper<T>
    {
        public DataWrapper(T data)
        {
            Data = data;
        }

        public string DataType => typeof(T).Name;
        public T Data { get; set; }
    }
}