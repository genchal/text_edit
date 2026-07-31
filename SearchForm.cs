using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace textEdit
{
    public partial class SearchForm : Form
    {
        //存储所有匹配项
        private List<Tuple<int, int, string>> _matches = new List<Tuple<int, int, string>>();
        //当前匹配索引
        private int _currentMatchIndex = -1;
        //当前搜索关键词
        private string _currentSearchText = "";
        //是否正则搜索
        private bool _isRegexSearch = false;

        public SearchForm()
        {
            InitializeComponent();
        }

        //=======关闭窗体
        private void ExitSearch_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void SearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        //======关闭窗体END

        //替换启用
        private void OnChange_CheckedChanged(object sender, EventArgs e)
        {
            Change.Enabled = OnChange.Checked;
            AllChange.Enabled = OnChange.Checked;
        }

        //不允许空白查找
        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            if (SearchText.Text != "")
                Search.Enabled = true;
            else
                Search.Enabled = false;
            //关键词改变时清空缓存
            _currentSearchText = "";
            _currentMatchIndex = -1;
        }

        //查找按钮 - 全文搜索
        private void Search_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        //执行全文搜索
        private void PerformSearch()
        {
            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;
            string rule = SearchText.Text;
            _isRegexSearch = OnRe.Checked;

            try
            {
                //检查是否需要重新搜索
                if (rule != _currentSearchText || _matches.Count == 0)
                {
                    //全文搜索
                    if (_isRegexSearch)
                        _matches = Find.GetAllRegexMatches(tb, rule);
                    else
                        _matches = Find.GetAllTextMatches(tb, rule);

                    _currentSearchText = rule;
                }

                if (_matches.Count == 0)
                {
                    MessageBox.Show("没有找到匹配项", "提示");
                    _currentMatchIndex = -1;
                    return;
                }

                //如果只有一个匹配项，且当前已经在该位置，则不跳转
                if (_matches.Count == 1)
                {
                    //检查当前是否已在该匹配位置
                    if (tb.SelectionStart == _matches[0].Item1 && tb.SelectionLength == _matches[0].Item2)
                    {
                        _currentMatchIndex = 0;
                        return; //已经在正确位置，不跳转
                    }
                }

                //定位到光标位置下的第一个匹配
                int cursorPos = tb.SelectionStart;
                _currentMatchIndex = Find.FindFirstMatchIndex(cursorPos, _matches);
                if (_currentMatchIndex >= 0)
                {
                    Find.NavigateToMatch(tb, _matches[_currentMatchIndex]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误: " + ex.Message);
            }
            tb.Focus();
        }

        //向上按钮 - 上一个匹配
        private void toUP_Click(object sender, EventArgs e)
        {
            NavigateToPrevious();
        }

        //向下按钮 - 下一个匹配
        private void toDown_Click(object sender, EventArgs e)
        {
            NavigateToNext();
        }

        //导航到上一个匹配
        private void NavigateToPrevious()
        {
            //如果没有缓存或关键词改变，先执行搜索
            if (_matches.Count == 0 || _currentSearchText != SearchText.Text)
            {
                PerformSearch();
                return;
            }

            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;

            if (_matches.Count == 0)
            {
                MessageBox.Show("没有找到匹配项", "提示");
                return;
            }

            //只有一个匹配项时不跳转
            if (_matches.Count == 1)
            {
                if (_currentMatchIndex == -1)
                {
                    _currentMatchIndex = 0;
                    Find.NavigateToMatch(tb, _matches[0]);
                }
                return;
            }

            if (_currentMatchIndex == -1)
            {
                //从最后一个开始
                _currentMatchIndex = _matches.Count - 1;
            }
            else
            {
                _currentMatchIndex = Find.GetPrevMatchIndex(_currentMatchIndex, _matches.Count);
            }

            Find.NavigateToMatch(tb, _matches[_currentMatchIndex]);
            tb.Focus();
        }

        //导航到下一个匹配
        private void NavigateToNext()
        {
            //如果没有缓存或关键词改变，先执行搜索
            if (_matches.Count == 0 || _currentSearchText != SearchText.Text)
            {
                PerformSearch();
                return;
            }

            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;

            if (_matches.Count == 0)
            {
                MessageBox.Show("没有找到匹配项", "提示");
                return;
            }

            //只有一个匹配项时不跳转
            if (_matches.Count == 1)
            {
                if (_currentMatchIndex == -1)
                {
                    _currentMatchIndex = 0;
                    Find.NavigateToMatch(tb, _matches[0]);
                }
                return;
            }

            if (_currentMatchIndex == -1)
            {
                //从光标位置开始
                int cursorPos = tb.SelectionStart;
                _currentMatchIndex = Find.FindFirstMatchIndex(cursorPos, _matches);
            }
            else
            {
                _currentMatchIndex = Find.GetNextMatchIndex(_currentMatchIndex, _matches.Count);
            }

            Find.NavigateToMatch(tb, _matches[_currentMatchIndex]);
            tb.Focus();
        }

        //单次替换
        private void Change_Click(object sender, EventArgs e)
        {
            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;

            //确保已经搜索并有选中内容
            if (_currentMatchIndex == -1 || _matches.Count == 0)
            {
                PerformSearch();
                return;
            }

            //验证当前选中的是否匹配
            if (tb.SelectedText.Length == 0 || !IsSelectedTextMatch())
            {
                //重新定位到当前匹配
                Find.NavigateToMatch(tb, _matches[_currentMatchIndex]);
            }

            if (tb.SelectedText.Length > 0)
            {
                tb.SelectedText = ChangeText.Text;
                //替换后更新匹配列表
                RefreshMatchesAfterReplace();
            }
        }

        //验证选中文本是否匹配搜索条件
        private bool IsSelectedTextMatch()
        {
            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;

            if (tb == null || string.IsNullOrEmpty(SearchText.Text))
                return false;

            if (_isRegexSearch)
            {
                Match m = Regex.Match(tb.SelectedText, SearchText.Text);
                return m.Success && m.Value == tb.SelectedText;
            }
            else
            {
                return tb.SelectedText.Equals(SearchText.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

        //替换后刷新匹配列表
        private void RefreshMatchesAfterReplace()
        {
            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;
            string rule = SearchText.Text;

            try
            {
                if (_isRegexSearch)
                    _matches = Find.GetAllRegexMatches(tb, rule);
                else
                    _matches = Find.GetAllTextMatches(tb, rule);

                _currentSearchText = rule;
                if (_matches.Count > 0)
                {
                    _currentMatchIndex = Find.FindFirstMatchIndex(tb.SelectionStart, _matches);
                    if (_currentMatchIndex >= 0)
                        Find.NavigateToMatch(tb, _matches[_currentMatchIndex]);
                }
                else
                {
                    _currentMatchIndex = -1;
                }
            }
            catch
            {
                _currentMatchIndex = -1;
            }
        }

        //全部替换
        private void AllChange_Click(object sender, EventArgs e)
        {
            Form1 f1 = (Form1)this.Owner;
            RichTextBox tb = f1.TB;
            try
            {
                if (_isRegexSearch)
                {
                    StringBuilder sb = new StringBuilder(Regex.Replace(tb.Text, SearchText.Text, ChangeText.Text));
                    tb.Text = sb.ToString();
                    sb.Clear();
                }
                else
                {
                    tb.Text = tb.Text.Replace(SearchText.Text, ChangeText.Text);
                }
                //替换后刷新
                _currentSearchText = "";
                _currentMatchIndex = -1;
                _matches.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误: " + ex.Message);
            }
        }
    }
}
