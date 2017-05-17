using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfExplorerTree
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TreeExplorer : UserControl
    {
        public TreeExplorer()
        {
            InitializeComponent();
        }

        private object dummyNode = null;

        /// <summary> 
        /// Identifies the Value dependency property. 
        /// </summary> 
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(
                "SelectedPath", typeof(string), typeof(TreeExplorer),
                new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedPathChanged)));

        /// <summary> 
        /// Gets or sets the value assigned to the control. 
        /// </summary> 
        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        bool handleCallback = true;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            BuildTree();
        }

        private void BuildTree()
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                foldersItem.Items.Add(item);
            }

            //string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            //const string win7Libraries = @"Microsoft\Windows\Libraries\";

            //string fullPathWin7Libraries = System.IO.Path.Combine(appDataFolder, win7Libraries);

            //string[] libraries = Directory.GetFiles(fullPathWin7Libraries, "*.library-ms");

            //foreach (string file in libraries)
            //{
            //    TreeViewItem item = new TreeViewItem();
            //    item.Header = System.IO.Path.GetFileNameWithoutExtension(file);
            //    item.Tag = file;
            //    item.FontWeight = FontWeights.Normal;
            //    item.Items.Add(dummyNode);
            //    item.Expanded += new RoutedEventHandler(folder_Expanded);
            //    foldersItem.Items.Add(item);
            //}
        }

        private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string path = (string)e.NewValue;

            TreeExplorer t = (TreeExplorer)d;

            if (!t.handleCallback || e.NewValue == null)
                return;

            TreeView tree = t.foldersItem;

            ItemCollection items = tree.Items;

            string subpath = path;

            int index = 0;

            string[] directories = path.Split(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });

            TreeViewItem lastItem = null;

            foreach (string dir in directories)
            {
                TreeViewItem item = SelectTreeViewItem(items, dir, index);
                if (item != null)
                {
                    lastItem = item;
                    items = item.Items;
                    item.IsExpanded = true;
                    //item.IsSelected = true;
                    t.folder_Expanded(item, null);                    
                }

                index++;
            }

            if (lastItem != null)
            {
                t.handleCallback = false;
                lastItem.IsSelected = true;                
                t.handleCallback = true;

                lastItem.BringIntoView();
            }
        }

        static TreeViewItem SelectTreeViewItem(ItemCollection items, string dir, int index)
        {
            TreeViewItem output = null;

            foreach (TreeViewItem item in items)
            {
                string comparing = index == 0 ? 
                    item.Tag.ToString().Substring(0, item.Tag.ToString().IndexOf(System.IO.Path.DirectorySeparatorChar)) : 
                    item.Header.ToString();

                if (comparing.Equals(dir))
                {
                    output = item;

                    break;
                }
            }

            return output;
        }


        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            StringBuilder sb = new StringBuilder();

            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if (temp == null)
                return;

            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                sb.Insert(0, temp1 + temp2);
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }

            handleCallback = false;
            SelectedPath = sb.ToString();
            handleCallback = true;
        }

    }
}
