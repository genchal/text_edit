using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace textEdit
{
    class TextFormat
    {
        public static Encoding GetTextFileEncodingType(string fileName)
        {
            Encoding encoding = Encoding.Default;
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            if (buffer.Length >= 3 && buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
            {
                encoding = Encoding.UTF8;
            }
            else if (buffer.Length >= 3 && buffer[0] == 254 && buffer[1] == 255 && buffer[2] == 0)
            {
                encoding = Encoding.BigEndianUnicode;
            }
            else if (buffer.Length >= 3 && buffer[0] == 255 && buffer[1] == 254 && buffer[2] == 65)
            {
                encoding = Encoding.Unicode;
            }
            else if (IsUTF8Bytes(buffer))
            {
                encoding = Encoding.UTF8;
            }
            else
            {
                encoding = DetectChineseEncoding(buffer);
            }

            return encoding;
        }

        private static Encoding DetectChineseEncoding(byte[] buffer)
        {
            Encoding gbk = Encoding.GetEncoding("GBK");
            Encoding utf8 = new UTF8Encoding(false, true);

            string gbkText = gbk.GetString(buffer);
            string utf8Text = null;
            try
            {
                utf8Text = utf8.GetString(buffer);
            }
            catch
            {
            }

            int gbkChineseCount = CountChineseCharacters(gbkText);
            int utf8ChineseCount = utf8Text != null ? CountChineseCharacters(utf8Text) : 0;
            int gbkReplacementCount = CountReplacementChars(gbkText);
            int utf8ReplacementCount = utf8Text != null ? CountReplacementChars(utf8Text) : int.MaxValue;

            if (utf8Text != null && utf8ChineseCount > gbkChineseCount && utf8ReplacementCount <= gbkReplacementCount)
            {
                return Encoding.UTF8;
            }

            return gbk;
        }

        private static int CountChineseCharacters(string text)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (IsChineseCharacter(c))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool IsChineseCharacter(char c)
        {
            return (c >= 0x4E00 && c <= 0x9FFF) ||
                   (c >= 0x3400 && c <= 0x4DBF) ||
                   (c >= 0x20000 && c <= 0x2A6DF) ||
                   (c >= 0xF900 && c <= 0xFAFF) ||
                   (c >= 0x2F800 && c <= 0x2FA1F) ||
                   c == 0x3000 || c == 0xFF0C || c == 0xFF1A || c == 0xFF01 || c == 0xFF1F ||
                   c == 0x3002 || c == 0x300C || c == 0x300D || c == 0x300E || c == 0x300F ||
                   c == 0x201C || c == 0x201D || c == 0x2018 || c == 0x2019;
        }

        private static int CountReplacementChars(string text)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == '\uFFFD')
                {
                    count++;
                }
            }
            return count;
        }

        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;
            byte curByte;
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
    }
}
