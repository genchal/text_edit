using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace textEdit
{
    class Find
    {
        //正则使用的关键标点符号
        public static List<string> ExcludeList = new List<string> { @"\", "$", "(", ")", "*", "+", ".", "[", "?", "^", "{", "|" };
        public static string GetNoRe(string oldStr)
        {
            string newStr = oldStr;
            foreach (string key in ExcludeList)
            {
                newStr = newStr.Replace(key, @"\" + key);
            }
            return newStr;
        }

        /// <summary>
        /// 获取全文所有正则匹配
        /// </summary>
        public static List<Tuple<int, int, string>> GetAllRegexMatches(RichTextBox tb, string rule)
        {
            List<Tuple<int, int, string>> matches = new List<Tuple<int, int, string>>();
            MatchCollection collection = Regex.Matches(tb.Text, rule);
            foreach (Match m in collection)
            {
                matches.Add(new Tuple<int, int, string>(m.Index, m.Length, m.Value));
            }
            return matches;
        }

        /// <summary>
        /// 获取全文所有文本匹配
        /// </summary>
        public static List<Tuple<int, int, string>> GetAllTextMatches(RichTextBox tb, string keyWord)
        {
            List<Tuple<int, int, string>> matches = new List<Tuple<int, int, string>>();
            int index = 0;
            while (index < tb.Text.Length)
            {
                int found = tb.Text.IndexOf(keyWord, index, StringComparison.OrdinalIgnoreCase);
                if (found == -1) break;
                matches.Add(new Tuple<int, int, string>(found, keyWord.Length, keyWord));
                index = found + keyWord.Length;
            }
            return matches;
        }

        /// <summary>
        /// 从光标位置找到第一个匹配项的索引
        /// </summary>
        public static int FindFirstMatchIndex(int cursorPos, List<Tuple<int, int, string>> matches)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Item1 >= cursorPos)
                    return i;
            }
            //如果光标位置之后没有匹配，返回第一个（循环）
            return matches.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// 导航到指定匹配项
        /// </summary>
        public static void NavigateToMatch(RichTextBox tb, Tuple<int, int, string> match)
        {
            //检查是否已经在该位置
            if (tb.SelectionStart == match.Item1 && tb.SelectionLength == match.Item2)
                return; //已经在正确位置，不跳转

            tb.SuspendLayout();
            try
            {
                tb.SelectionStart = match.Item1;
                tb.SelectionLength = match.Item2;
                tb.ScrollToCaret();
            }
            finally
            {
                tb.ResumeLayout();
            }
        }

        /// <summary>
        /// 获取上一个匹配项索引
        /// </summary>
        public static int GetPrevMatchIndex(int currentIndex, int totalCount)
        {
            if (currentIndex <= 0)
                return totalCount - 1; //循环到最后
            return currentIndex - 1;
        }

        /// <summary>
        /// 获取下一个匹配项索引
        /// </summary>
        public static int GetNextMatchIndex(int currentIndex, int totalCount)
        {
            if (currentIndex >= totalCount - 1)
                return 0; //循环到开头
            return currentIndex + 1;
        }
    }
}
