﻿using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;

namespace ResxEditor
{
    public partial class ResxEditor : Form
    {

        #region 变量

        public static string ResxEng = "";
        public static string ResxSC = "";
        public static string ResxTC = "";

        public static int NowLanguage = 0; // 0 = eng, 1 = sc, 2 = tc

        public Dictionary<string, string> EngDictionary = new();
        public Dictionary<string, string> SCDictionary= new();
        public Dictionary<string, string> TCDictionary = new();

        private DataTable DataTable; // 主表格
        private DataGridViewRow CurrentRow; // 当前选中行
        private ArrayList[] FilterLists = new ArrayList[7]; // 筛选器
        private Setting SettingForm = new(); // 单例模式设置

        private bool IsEdited = false; // 是否被编辑过
        private bool IsChangingLanguage = false; // 是否正在切换语言

        private const string Key = "Key";
        private const string Eng = "Eng";
        private const string SC = "SC";
        private const string TC = "TC";
        private static string EngString;
        private static string SCString;
        private static string TCString;

        #endregion

        #region 方法

        public ResxEditor()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitResource();
            InitINIFile();
            InitFilterLists();
            InitEvent();
            SetLanguageString();
            LoadGridData();
            IsChangingLanguage = true;
            SetLanguage();
            this.LabelCount.Text = Convert.ToString(DataTable.Rows.Count);
        }

        private void SetLanguage()
        {
            if (IsChangingLanguage)
            {
                IsChangingLanguage = false;
                SetLanguageString();
                if (ChangeLanguage.Items.Count > 0)
                {
                    ChangeLanguage.Items[0] = EngString;
                    ChangeLanguage.Items[1] = SCString;
                    ChangeLanguage.Items[2] = TCString;
                }
                else
                {
                    ChangeLanguage.Items.Add(EngString);
                    ChangeLanguage.Items.Add(SCString);
                    ChangeLanguage.Items.Add(TCString);
                }
                ChangeLanguage.SelectedItem = ChangeLanguage.Items[NowLanguage];
                Text = GetString("editor.main.title");
                Reload.Text = GetString("editor.main.reload");
                Save.Text = GetString("editor.main.save");
                Clear.Text = GetString("editor.main.clear");
                Setting.Text = GetString("editor.main.setting");
                LabelKey.Text = GetString("editor.main.key");
                LabelEng.Text = GetString("editor.main.label.english");
                LabelSC.Text = GetString("editor.main.label.schinese");
                LabelTC.Text = GetString("editor.main.label.tchinese");
                Add.Text = GetString("editor.main.add");
                Rename.Text = GetString("editor.main.rename");
            }
        }

        private void SetLanguageString()
        {
            EngString = GetString("editor.main.english");
            SCString = GetString("editor.main.schinese");
            TCString = GetString("editor.main.tchinese");
        }

        public void SetSettingsLanguage()
        {
            Action action = () =>
            {
                SettingForm.Text = GetString("editor.main.setting");
                SettingForm.LoadEng.Text = GetString("editor.main.setting.l");
                SettingForm.LoadSC.Text = GetString("editor.main.setting.l");
                SettingForm.LoadTC.Text = GetString("editor.main.setting.l");
                SettingForm.ClearEng.Text = GetString("editor.main.setting.c");
                SettingForm.ClearSC.Text = GetString("editor.main.setting.c");
                SettingForm.ClearTC.Text = GetString("editor.main.setting.c");
                SettingForm.Tips.Text = GetString("editor.main.setting.tip");
                SettingForm.Confirm.Text = GetString("editor.main.setting.confirm");
            };
            Setting.Invoke(action);
        }

        private string GetString(string key)
        {
            switch(NowLanguage)
            {
                case 0:
                    if (EngDictionary.ContainsKey(key))
                        return EngDictionary[key];
                    break;
                case 1:
                    if (SCDictionary.ContainsKey(key))
                        return SCDictionary[key];
                    break;
                case 2:
                    if (TCDictionary.ContainsKey(key))
                        return TCDictionary[key];
                    break;
            }
            return "";
        }

        private void InitResource()
        {
            ResXResourceReader ResxReaderEng = new("Eng.resx");
            ResXResourceReader ResxReaderSC = new("SChinese.resx");
            ResXResourceReader ResxReaderTC = new("TChinese.resx");
            foreach (DictionaryEntry Entry in ResxReaderEng)
            {
                EngDictionary.Add(Entry.Key.ToString(), Entry.Value.ToString());
            }
            foreach (DictionaryEntry Entry in ResxReaderSC)
            {
                SCDictionary.Add(Entry.Key.ToString(), Entry.Value.ToString());
            }
            foreach (DictionaryEntry Entry in ResxReaderTC)
            {
                TCDictionary.Add(Entry.Key.ToString(), Entry.Value.ToString());
            }
        }

        private void InitINIFile()
        {
            if (ExistINIFile())
            {
                ResxEng = ReadINI("Resource", "English");
                ResxSC = ReadINI("Resource", "SChinese");
                ResxTC = ReadINI("Resource", "TChinese");
            }
            else
            {
                WriteINI("Resource", "SChinese", "");
                WriteINI("Resource", "TChinese", "");
                WriteINI("Resource", "English", "");
                MessageBox.Show("Initialization completed, please set the file path!");
                SettingForm.Show();
                SetSettingsLanguage();
            }
        }

        private void InitFilterLists()
        {
            for (int i = 0; i < 7; i++)
            {
                FilterLists[i] = new ArrayList();
            }
        }

        private void InitEvent()
        {
            FormClosing += ResxEditor_FormClosing;
            KeyDown += ResxEditor_KeyDown;
            Filter1.SelectionChangeCommitted += Filter_Selected;
            Filter2.SelectionChangeCommitted += Filter_Selected;
            Filter3.SelectionChangeCommitted += Filter_Selected;
            Filter4.SelectionChangeCommitted += Filter_Selected;
            Filter5.SelectionChangeCommitted += Filter_Selected;
            Filter6.SelectionChangeCommitted += Filter_Selected;
            Filter7.SelectionChangeCommitted += Filter_Selected;
            Reload.Click += Reload_Click;
            Save.Click += Save_Click;
            Clear.Click += ClearGrid_Click;
            Setting.Click += Setting_Click;
            Add.Click += AddNew_Click;
            Rename.Click += Rename_Click;
            Delete.Click += Delete_Click;
            GridView.ColumnHeaderMouseClick += GridView_ColumnHeaderClick;
            GridView.CellValueChanged += GridView_CellChanged;
            GridView.CellClick += GridView_CellClick;
            Copyright.LinkClicked += Copyright_LinkClicked;
            ChangeLanguage.SelectedIndexChanged += ChangeLanguage_Selected;
            ChangeLanguage.Click += ChangeLanguage_Click;
        }

        private void SetCurrentTextBox()
        {
            this.TextKey.Text = Convert.ToString(CurrentRow.Cells[0].Value);
            this.TextEng.Text = Convert.ToString(CurrentRow.Cells[1].Value);
            this.TextSC.Text = Convert.ToString(CurrentRow.Cells[2].Value);
            this.TextTC.Text = Convert.ToString(CurrentRow.Cells[3].Value);
        }

        private void LoadGridData()
        {
            // 创建数据表
            DataTable = new DataTable();
            DataTable.Columns.Add(Key, typeof(string));
            DataTable.Columns.Add(Eng, typeof(string));
            DataTable.Columns.Add(SC, typeof(string));
            DataTable.Columns.Add(TC, typeof(string));

            // 读取数据
            ResXResourceReader ResxReaderEng = new(ResxEng);
            ResXResourceReader ResxReaderSC = new(ResxSC);
            ResXResourceReader ResxReaderTC = new(ResxTC);

            // 将数据加入到数据表
            int intRow = 0; // 当前所在行下标
            try
            {
                foreach (DictionaryEntry Entry in ResxReaderEng)
                {
                    // 遍历ENG，加入KEY和ENG
                    DataRow Row = DataTable.NewRow();
                    string strKey = Entry.Key.ToString();
                    Row[Key] = strKey;
                    Row[Eng] = Entry.Value.ToString();
                    DataTable.Rows.Add(Row);
                    // 获得KEY的筛选项
                    string[] temp = strKey.Split('.');
                    for (int i = 0; i < temp.Length && i < 7; i++)
                    {
                        if (temp[i] != "" && !FilterLists[i].Contains(temp[i]))
                        {
                            FilterLists[i].Add(temp[i]);
                        }
                    }
                }
                foreach (DictionaryEntry Entry in ResxReaderSC)
                {
                    DataRow Row = DataTable.Rows[intRow];
                    Row[SC] = Entry.Value.ToString();
                    intRow++;
                }
                intRow = 0;
                foreach (DictionaryEntry Entry in ResxReaderTC)
                {
                    DataRow Row = DataTable.Rows[intRow];
                    Row[TC] = Entry.Value.ToString();
                    intRow++;
                }

                // 默认排序
                DefaultSort();

                // 显示
                LabelCount.Text = Convert.ToString(DataTable.Rows.Count);
                GridView.DataSource = DataTable.DefaultView;
                if (GridView.Rows.Count > 0)
                {
                    CurrentRow = GridView.Rows[0];
                    SetCurrentTextBox();
                }

                // 加入筛选选项
                SortFilteLists(FilterLists);
                InitComboItem();

                IsEdited = false;
                Text = "Resource Editor";
            }
            catch (Exception objE)
            {
                Console.WriteLine(objE.StackTrace);
                ClearGird();
            }
            finally
            {
                ResxReaderEng.Close();
                ResxReaderSC.Close();
                ResxReaderTC.Close();
            }
        }

        private void DefaultSort()
        {
            // 默认排序
            DataView View = DataTable.DefaultView;
            View.Sort = "Key asc";
            DataTable = View.ToTable();
        }

        private void ClearGird()
        {
            // 清空表格
            this.DataTable.Rows.Clear();
            this.GridView.DataSource = this.DataTable;
            this.LabelCount.Text = Convert.ToString(DataTable.Rows.Count);
            IsEdited = false;
            this.Text = "Resource Editor";
        }

        private void CellChanged()
        {
            // 修改更改状态
            IsEdited = true;
            this.Text = "* Resource Editor";
        }

        private void ReloadAll()
        {
            // 重新加载表格
            if (IsEdited)
            {
                if (MessageBox.Show("未保存的数据将丢失，是否重新加载？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    LoadGridData();
            }
            else LoadGridData();
        }

        private void ClearAll()
        {
            // 清空表格
            if (IsEdited)
            {
                if (MessageBox.Show("未保存的数据将丢失，是否清空表格？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    ClearGird();
            }
            else ClearGird();
        }

        /**
         * 引入读写INI文件依赖
         */
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void WriteINI(string Section, string Key, string Value, string FileName = @"ResxConfig.ini")
        {
            WritePrivateProfileString(Section, Key, Value, System.Environment.CurrentDirectory.ToString() + @"\" + FileName);
        }

        public static string ReadINI(string Section, string Key, string FileName = @"ResxConfig.ini")
        {
            StringBuilder str = new(256);
            _ = GetPrivateProfileString(Section, Key, "", str, 256, System.Environment.CurrentDirectory.ToString() + @"\" + FileName);
            return str.ToString();
        }

        public static bool ExistINIFile(string FileName = @"ResxConfig.ini")
        {
            return File.Exists(System.Environment.CurrentDirectory.ToString() + @"\" + FileName);
        }

        #endregion

        #region 操作数据

        private int AddNew()
        {
            // 添加一行
            try
            {
                if (this.TextKey.Text.Trim() == "" ||
                    this.TextEng.Text.Trim() == "" ||
                    this.TextSC.Text.Trim() == "" ||
                    this.TextTC.Text.Trim() == "")
                {
                    return 0;
                }
                string temp = Convert.ToString(this.TextKey.Text);
                DataGridViewRow Row = IsExistKey(temp);
                if (Row != null)
                {
                    GridView.CurrentCell = GridView.Rows[Row.Index].Cells[0];
                    return RenameRow(IsExistKey(temp)); // 存在相同Key就重命名
                }
                else
                {
                    // 不存在新增一行
                    DataRow NewRow = DataTable.NewRow();
                    NewRow[Key] = this.TextKey.Text;
                    NewRow[Eng] = this.TextEng.Text;
                    NewRow[SC] = this.TextSC.Text;
                    NewRow[TC] = this.TextTC.Text;
                    DataTable.Rows.Add(NewRow);
                    DefaultSort();
                    this.LabelCount.Text = Convert.ToString(DataTable.Rows.Count);
                    this.GridView.DataSource = this.DataTable.DefaultView;
                }
            }
            catch (Exception objE)
            {
                Console.WriteLine(objE.StackTrace);
                return 0;
            }
            return 1;
        }

        private int RenameRow(DataGridViewRow Row)
        {
            // 重命名 指定行
            try
            {
                if (this.TextKey.Text.Trim() == "" ||
                    this.TextEng.Text.Trim() == "" ||
                    this.TextSC.Text.Trim() == "" ||
                    this.TextTC.Text.Trim() == "")
                {
                    return 0;
                }
                Row.Cells[0].Value = this.TextKey.Text;
                Row.Cells[1].Value = this.TextEng.Text;
                Row.Cells[2].Value = this.TextSC.Text;
                Row.Cells[3].Value = this.TextTC.Text;
            }
            catch (Exception objE)
            {
                Console.WriteLine(objE.StackTrace);
                return 0;
            }
            return 2;
        }

        private int RenameRow()
        {
            // 重命名
            try
            {
                if (this.TextKey.Text.Trim() == "" ||
                    this.TextEng.Text.Trim() == "" ||
                    this.TextSC.Text.Trim() == "" ||
                    this.TextTC.Text.Trim() == "")
                {
                    return 0;
                }
                string temp = Convert.ToString(this.TextKey.Text);
                DataGridViewRow Row = IsExistKey(temp);
                if (Row != null)
                {
                    GridView.CurrentCell = GridView.Rows[Row.Index].Cells[0];
                    return RenameRow(IsExistKey(temp)); // 存在相同Key就重命名那一行
                }
                else
                {
                    // 不存在重命名这行
                    CurrentRow = GridView.CurrentRow;
                    CurrentRow.Cells[0].Value = this.TextKey.Text;
                    CurrentRow.Cells[1].Value = this.TextEng.Text;
                    CurrentRow.Cells[2].Value = this.TextSC.Text;
                    CurrentRow.Cells[3].Value = this.TextTC.Text;
                }
            }
            catch (Exception objE)
            {
                Console.WriteLine(objE.StackTrace);
                return 0;
            }
            return 1;
        }

        private int DeleteRow(DataGridViewRow Row)
        {
            // 删除 指定行
            try
            {
                GridView.Rows.Remove(Row);
                GridView.Refresh();
                DefaultSort();
                this.LabelCount.Text = Convert.ToString(DataTable.Rows.Count);
                this.GridView.DataSource = this.DataTable.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace + "\n" + e.StackTrace);
                return 0;
            }
            return 1;
        }

        private int DeleteRow()
        {
            // 删除
            try
            {
                string temp = Convert.ToString(this.TextKey.Text);
                DataGridViewRow Row = IsExistKey(temp);
                if (Row != null)
                {
                    GridView.CurrentCell = GridView.Rows[Row.Index].Cells[0];
                    return DeleteRow(IsExistKey(temp)); // 存在相同Key就删除那一行
                }
                else
                {
                    // 不存在删除这行
                    return 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
                return 0;
            }
        }

        private bool SaveAll()
        {
            // 保存所有更改
            try
            {
                using ResXResourceWriter ResxWriterEng = new(ResxEng),
                    ResxWriterSC = new(ResxSC),
                    ResxWriterTC = new(ResxTC);
                DataTable.DefaultView.RowFilter = "";
                for (int i = 0; i < DataTable.Rows.Count; i++)
                {
                    string strKey = Convert.ToString(GridView.Rows[i].Cells[0].Value);
                    string strEng = Convert.ToString(GridView.Rows[i].Cells[1].Value);
                    string strSC = Convert.ToString(GridView.Rows[i].Cells[2].Value);
                    string strTC = Convert.ToString(GridView.Rows[i].Cells[3].Value);
                    ResxWriterEng.AddResource(strKey, strEng);
                    ResxWriterSC.AddResource(strKey, strSC);
                    ResxWriterTC.AddResource(strKey, strTC);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
                return false;
            }
            return true;
        }

        private DataGridViewRow IsExistKey(String strkey)
        {
            // 判断是否存在相同key，存在返回该行
            foreach (DataGridViewRow objItem in GridView.Rows)
            {
                if (strkey == Convert.ToString(objItem.Cells[0].Value)) return objItem;
            }
            return null;
        }

        #endregion

        #region 筛选器

        private void Filter(ComboBox objCombo)
        {
            // Filter
            if (Filter1.SelectedItem.Equals("*"))
            {
                Filter2.SelectedItem = "*";
                Filter3.SelectedItem = "*";
                Filter4.SelectedItem = "*";
                Filter5.SelectedItem = "*";
                Filter6.SelectedItem = "*";
                Filter7.SelectedItem = "*";
                DataTable.DefaultView.RowFilter = "";
                InitComboItem();
            }
            else
            {
                // 设置筛选器表达式
                DataTable.DefaultView.RowFilter = "Key like '" + BuildComboboxFilter(Filter1) +
                    BuildComboboxFilter(Filter2) + BuildComboboxFilter(Filter3) +
                    BuildComboboxFilter(Filter4) + BuildComboboxFilter(Filter5) +
                    BuildComboboxFilter(Filter6) + BuildComboboxFilter(Filter7) + "%'";
                // 更改选项
                UpdateComboFilterItem(SelectedComboBox(objCombo));
            }
        }

        private string BuildComboboxFilter(ComboBox objCombo)
        {
            // 将每个ComboBox的选项转化为表达式
            string strKey = "";
            string Filter = "";
            strKey = objCombo.SelectedItem.ToString();
            if (strKey != "*" && strKey != "")
            {
                if (objCombo == Filter1) Filter = "" + strKey + "";
                else Filter = "." + strKey + "";
            }
            return Filter;
        }

        private void SortFilteLists(ArrayList[] objLists)
        {
            // 排序
            for (int i = 0; i < 7; i++)
            {
                objLists[i].Sort();
            }
        }

        private void SetDefaultComboItem(int pos)
        {
            // 判断点击的Combo是哪一个，重置他的下一个Combo的选项
            switch (pos)
            {
                case 1:
                    Filter2.Items.Clear();
                    Filter2.Items.Add("*");
                    Filter2.SelectedIndex = 0;
                    ArrayList[] Lists = GetComboFilterItem();
                    for (int i = 0; i < Lists[pos].Count; i++)
                    {
                        Filter2.Items.Add(Lists[pos][i]);
                    }
                    goto C2;
                case 2:
                C2: Filter3.Items.Clear();
                    Filter3.Items.Add("*");
                    Filter3.SelectedIndex = 0;
                    Lists = GetComboFilterItem(pos);
                    for (int i = 0; i < Lists[pos].Count; i++)
                    {
                        Filter3.Items.Add(Lists[pos][i]);
                    }
                    goto C3;
                case 3:
                C3: Filter4.Items.Clear();
                    Filter4.Items.Add("*");
                    Filter4.SelectedIndex = 0;
                    Lists = GetComboFilterItem(pos);
                    for (int i = 0; i < Lists[pos].Count; i++)
                    {
                        Filter4.Items.Add(Lists[pos][i]);
                    }
                    goto C4;
                case 4:
                C4: Filter5.Items.Clear();
                    Filter5.Items.Add("*");
                    Filter5.SelectedIndex = 0;
                    Lists = GetComboFilterItem(pos);
                    for (int i = 0; i < Lists[pos].Count; i++)
                    {
                        Filter5.Items.Add(Lists[pos][i]);
                    }
                    goto C5;
                case 5:
                C5: Filter6.Items.Clear();
                    Filter6.Items.Add("*");
                    Filter6.SelectedIndex = 0;
                    Lists = GetComboFilterItem(pos);
                    for (int i = 0; i < Lists[pos].Count; i++)
                    {
                        Filter6.Items.Add(Lists[pos][i]);
                    }
                    goto C6;
                case 6:
                C6: Filter7.Items.Clear();
                    Filter7.Items.Add("*");
                    Filter7.SelectedIndex = 0;
                    Lists = GetComboFilterItem(pos);
                    for (int i = 0; i < Lists[pos].Count; i++)
                    {
                        Filter7.Items.Add(Lists[pos][i]);
                    }
                    goto C7;
                case 7:
                C7: break;
            }
        }

        private void InitComboItem()
        {
            // 给下拉菜单加入选项 初始化和筛选*时使用
            Filter1.Items.Clear();
            Filter2.Items.Clear();
            Filter3.Items.Clear();
            Filter4.Items.Clear();
            Filter5.Items.Clear();
            Filter6.Items.Clear();
            Filter7.Items.Clear();
            Filter1.Items.Add("*");
            Filter2.Items.Add("*");
            Filter3.Items.Add("*");
            Filter4.Items.Add("*");
            Filter5.Items.Add("*");
            Filter6.Items.Add("*");
            Filter7.Items.Add("*");
            Filter1.SelectedIndex = 0;
            Filter2.SelectedIndex = 0;
            Filter3.SelectedIndex = 0;
            Filter4.SelectedIndex = 0;
            Filter5.SelectedIndex = 0;
            Filter6.SelectedIndex = 0;
            Filter7.SelectedIndex = 0;
            for (int i = 0; i < FilterLists[0].Count; i++)
            {
                Filter1.Items.Add(FilterLists[0][i]);
            }
            for (int i = 0; i < FilterLists[1].Count; i++)
            {
                Filter2.Items.Add(FilterLists[1][i]);
            }
            for (int i = 0; i < FilterLists[2].Count; i++)
            {
                Filter3.Items.Add(FilterLists[2][i]);
            }
            for (int i = 0; i < FilterLists[3].Count; i++)
            {
                Filter4.Items.Add(FilterLists[3][i]);
            }
            for (int i = 0; i < FilterLists[4].Count; i++)
            {
                Filter5.Items.Add(FilterLists[4][i]);
            }
            for (int i = 0; i < FilterLists[5].Count; i++)
            {
                Filter6.Items.Add(FilterLists[5][i]);
            }
            for (int i = 0; i < FilterLists[6].Count; i++)
            {
                Filter7.Items.Add(FilterLists[6][i]);
            }
        }

        private ArrayList[] GetComboFilterItem()
        {
            // 通过表格当前展示的行，获取Combo筛选项
            ArrayList[] Lists = new ArrayList[7];
            for (int i = 0; i < 7; i++)
            {
                Lists[i] = new ArrayList();
            }
            foreach (DataGridViewRow Row in GridView.Rows)
            {
                string[] strTemp = Row.Cells[0].Value.ToString().Split('.');
                for (int i = 0; i < strTemp.Length && i < 7; i++)
                {
                    if (strTemp[i].Trim() != "" && !Lists[i].Contains(strTemp[i]))
                    {
                        Lists[i].Add(strTemp[i]);
                    }
                }
            }
            SortFilteLists(Lists);
            return Lists;
        }

        private ArrayList[] GetComboFilterItem(int pos)
        {
            // 按位置 获取Combo筛选项
            ArrayList[] Lists = new ArrayList[7];
            for (int i = 0; i < 7; i++)
            {
                Lists[i] = new ArrayList();
            }
            foreach (DataGridViewRow Row in GridView.Rows)
            {
                String[] temp = Row.Cells[0].Value.ToString().Split('.');
                for (int i = pos; i < temp.Length && i < 7; i++)
                {
                    if (temp[i].Trim() != "" && !Lists[i].Contains(temp[i]))
                    {
                        Lists[i].Add(temp[i]);
                    }
                }
            }
            SortFilteLists(Lists);
            return Lists;
        }

        private void UpdateComboFilterItem(int pos)
        {
            // 从指定位置开始清空选项 然后加入选项
            SetDefaultComboItem(pos);
        }

        private int SelectedComboBox(ComboBox Box)
        {
            // 判断点击的Combox，返回位置
            if (Box.Equals(Filter1)) return 1;
            else if (Box.Equals(Filter2)) return 2;
            else if (Box.Equals(Filter3)) return 3;
            else if (Box.Equals(Filter4)) return 4;
            else if (Box.Equals(Filter5)) return 5;
            else if (Box.Equals(Filter6)) return 6;
            else return 7;
        }

        #endregion

        #region 事件

        private void ResxEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 当数据被更改后，未保存不能直接退出
            if (IsEdited)
            {
                if (MessageBox.Show("有被更改的数据尚未保存，是否退出？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    e.Cancel = false;
                else e.Cancel = true;
            }
        }

        private void ResxEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)   //Ctrl + S
            {
                Save_Click();
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                ReloadAll();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                ClearAll();
            }
            else if (e.Control && e.KeyCode == Keys.T)
            {
                SettingForm.Show();
                SetSettingsLanguage();
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                AddNew_Click();
            }
            else if (e.Control && e.KeyCode == Keys.R)
            {
                Rename_Click();
            }
            else if (e.Control && e.KeyCode == Keys.M)
            {
                Process.Start(new ProcessStartInfo("https://mili.cyou") { UseShellExecute = true });
            }
            else if (e.Alt && e.KeyCode == Keys.F4)
            {
                if (IsEdited)
                {
                    if (MessageBox.Show("有被更改的数据尚未保存，是否退出？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        this.Dispose();
                }
            }
        }

        private void GridView_ColumnHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 点击列头排序事件
            DataView view = DataTable.DefaultView;
            if (GridView.SortedColumn != null) // 判断是否有排序
            {
                view.Sort = GridView.SortedColumn.DataPropertyName + (GridView.SortOrder == SortOrder.Ascending ? " asc" : " desc");
            }
            DataTable = view.ToTable();
        }

        private void GridView_CellChanged(object sender, EventArgs e)
        {
            // 单元格数据被更改事件
            CellChanged();
        }

        private void GridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 获取当前选中行
            CurrentRow = GridView.CurrentRow;
            SetCurrentTextBox();
        }

        private void Filter_Selected(object serder, EventArgs e)
        {
            // 筛选器事件
            Filter(serder as ComboBox);
        }

        private void ChangeLanguage_Click(object serder, EventArgs e)
        {
            IsChangingLanguage = true;
        }

        private void ChangeLanguage_Selected(object serder, EventArgs e)
        {
            SetLanguageString();
            NowLanguage = ChangeLanguage.SelectedIndex;
            SetLanguage();
        }

        #endregion

        #region Reload Clear按钮

        private void Reload_Click(object sender, EventArgs e)
        {
            ReloadAll();
        }

        private void ClearGrid_Click(object sender, EventArgs e)
        {
            ClearAll();
        }

        #endregion

        #region AddNew Rename Delete SavaAll按钮

        private void Save_Click()
        {
            if (IsEdited)
            {
                if (SaveAll())
                {
                    IsEdited = false;
                    this.Text = "Resource Editor";
                    MessageBox.Show("保存成功", "提示", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show("保存失败", "提示", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("未修改数据", "提示", MessageBoxButtons.OK);
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Save_Click();
        }

        private void Rename_Click()
        {
            if (RenameRow() == 1)
            {
                CellChanged();
                MessageBox.Show("重命名成功", "提示", MessageBoxButtons.OK);
            }
            else if (RenameRow() == 2)
            {
                CellChanged();
                MessageBox.Show("已存在相同Key，已重命名那一行的数据", "提示", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("重命名失败", "提示", MessageBoxButtons.OK);
            }
        }

        private void Rename_Click(object sender, EventArgs e)
        {
            Rename_Click();
        }

        private void Delete_Click()
        {
            if (MessageBox.Show("确定删除该行？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (DeleteRow() == 1)
                {
                    CellChanged();
                    MessageBox.Show("删除成功", "提示", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show("删除失败", "提示", MessageBoxButtons.OK);
                }
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            Delete_Click();
        }

        private void AddNew_Click()
        {
            if (AddNew() == 1)
            {
                CellChanged();
                MessageBox.Show("添加成功", "提示", MessageBoxButtons.OK);
            }
            else if (AddNew() == 2)
            {
                CellChanged();
                MessageBox.Show("已存在相同Key，已重命名那一行的数据", "提示", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("添加失败", "提示", MessageBoxButtons.OK);
            }
        }

        private void AddNew_Click(object sender, EventArgs e)
        {
            AddNew_Click();
        }

        private void Setting_Click(object sender, EventArgs e)
        {
            SettingForm.Show();
            SetSettingsLanguage();
        }

        private void Copyright_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://mili.cyou") { UseShellExecute = true });
        }

        #endregion

    }
}
