using System;
using System.Collections.Generic;
using System.Text;

namespace System.Helper
{
    public class ConvertHelper
    {

        /// <summary>
        /// 将数组转换为字符串
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static string ConvertToStringFromIntArray(IEnumerable<int> arr)
        {
            return ConvertToStringFromIntArray(arr, true, "");
        }
        /// <summary>
        ///  将数组转换为字符串
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="IgnoreZero">是否匆略数组中的零值</param>
        /// <returns></returns>
        public static string ConvertToStringFromIntArray(IEnumerable<int> arr, bool IgnoreZero)
        {
            return ConvertToStringFromIntArray(arr, IgnoreZero, "");
        }
        /// <summary>
        /// 将数组转换为字符串
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="IgnoreZero">是否匆略数组中的零值</param>
        /// <param name="SpaceTag">指定个间隔字符串</param>
        /// <returns></returns>
        public static string ConvertToStringFromIntArray(IEnumerable<int> arr, bool IgnoreZero, string SpaceTag)
        {
            if (arr == null) return "";
            if (string.IsNullOrEmpty(SpaceTag))
            {
                SpaceTag = ",";
            }
            StringBuilder sb = new StringBuilder();
            foreach (int i in arr)
            {
                if (i == 0 && IgnoreZero) continue;
                if (sb.Length > 0)
                {
                    sb.Append(SpaceTag + i.ToString());
                }
                else
                {
                    sb.Append(i.ToString());
                }

            }
            return sb.ToString();
        }
 



        /// <summary>
        /// 字符串转换为整型数组
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IEnumerable<int> ConvertToIntArrayFromString(string src)
        {
            return ConvertToIntArrayFromString(src, true, "");
        }
        /// <summary>
        /// 字符串转换为整型数组
        /// </summary>
        /// <param name="src"></param>
        /// <param name="IgnoreZero">是否匆略字符串中的零值</param>
        /// <returns></returns>
        public static IEnumerable<int> ConvertToIntArrayFromString(string src, bool IgnoreZero)
        {
            return ConvertToIntArrayFromString(src, IgnoreZero, "");
        }
        /// <summary>
        /// 字符串转换为整型数组
        /// </summary>
        /// <param name="src"></param>
        /// <param name="IgnoreZero">是否匆略字符串中的零值</param>
        /// <param name="SpaceTag">指定字符串中的间隔字符串</param>
        /// <returns></returns>
        public static IEnumerable<int> ConvertToIntArrayFromString(string src, bool IgnoreZero, string SpaceTag)
        {
            if (string.IsNullOrEmpty(src)) return null;
            if (string.IsNullOrEmpty(SpaceTag))
            {
                SpaceTag = ",";
            }
            string[] arr = src.Split(new string[] { SpaceTag }, StringSplitOptions.RemoveEmptyEntries);
            List<int> list = new List<int>();
            foreach (string i in arr)
            {
                int temp = 0;
                if (Int32.TryParse(i, out temp))
                {
                    if (temp == 0 && IgnoreZero) continue;
                    list.Add(temp);
                }
            }
            return list;
        }
        /// <summary>
        /// 返回一个以逗号隔的ID列表
        /// 如果参数ListId为空或所有元数为0则抛出ArgumentException异常
        /// </summary>
        /// <param name="ListId"></param>
        /// <returns></returns>
        public static string ValidateListId(IEnumerable<int> ListId)
        {
            string IdStr = ConvertToStringFromIntArray(ListId);
            if (string.IsNullOrEmpty(IdStr) || IdStr.Trim()=="")
            {
                throw new ArgumentException("至少要指定一个ID");
            }
            return IdStr;
        }
        /// <summary>
        /// 转换到客户端字符串，将”"\n,\r“转换为客户端的转义字符
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ConvertToClientString(string s)
        {
            return (s ?? "").Trim().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
