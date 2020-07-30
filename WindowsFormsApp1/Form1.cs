using Equin.ApplicationFramework;
using ImageSprites;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private BindingListView<FileInfoDisplayName> sourceFiles;
        private string[] extensions = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

        public Form1()
        {
            InitializeComponent();

            lbSourceFiles.DisplayMember = "DisplayName";
            lbTarget.DisplayMember = "DisplayName";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();

            if (result == DialogResult.OK)
            {
                var filePath = ofd.FileName;
                var fileInfo = new FileInfo(filePath);

                txtDirectory.Text = fileInfo.Directory.FullName;

                var files = fileInfo.Directory.GetFiles("*", SearchOption.AllDirectories)
                    .Where(f => extensions.Any(ext => ext == f.Extension))
                    .Select(s =>
                    {
                        var displayName = s.FullName.Replace(txtDirectory.Text, "");
                        return new FileInfoDisplayName() { File = s, DisplayName = displayName };
                    }).ToList();

                sourceFiles = new BindingListView<FileInfoDisplayName>(files);
                lbSourceFiles.DataSource = sourceFiles;
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            Filter();
        }

        private void Filter()
        {
            var targetFiles = new List<FileInfoDisplayName>();
            foreach (FileInfoDisplayName f in lbTarget.Items)
            {
                targetFiles.Add(f);
            }

            sourceFiles.ApplyFilter(f => f.File.Name.Contains(txtFilter.Text) && !targetFiles.Any(t => t.DisplayName == f.DisplayName));
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var item = (lbSourceFiles.SelectedItem as ObjectView<FileInfoDisplayName>)?.Object;

            if (item == null)
            {
                return;
            }

            lbTarget.Items.Add(item);
            Filter();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var item = lbTarget.SelectedItem as FileInfoDisplayName;

            if (item == null)
            {
                return;
            }

            var index = lbTarget.SelectedIndex;
            lbTarget.Items.Remove(item);

            while (index >= lbTarget.Items.Count)
            {
                index--;
            }

            lbTarget.SelectedIndex = index;

            Filter();
        }

        private SpriteImageGenerator _generator = new SpriteImageGenerator();

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var _artifacts = new DirectoryInfo(".\\").FullName;

            var spritesDirName = "Sprites";
            if (!Directory.Exists(spritesDirName))
            {
                Directory.CreateDirectory(spritesDirName);
            }

            var _fileName = Path.Combine(_artifacts, spritesDirName, $"{txtSpriteName.Text}.sprite");

            var images = new List<string>();
            foreach (FileInfoDisplayName f in lbTarget.Items)
            {
                images.Add(f.File.FullName);
            }

            var document = new SpriteDocument(_fileName, images);
            document.Orientation = ImageSprites.Orientation.Horizontal;
            document.Padding = 20;
            document.Output = ImageType.Png;
            document.Stylesheet = Stylesheet.Css;
            document.CustomStyles["display"] = "block";
            document.CustomStyles.Add("margin", "0");

            var task = Task.Run(() => _generator.Generate(document));
            task.Wait();

            var documentTask = Task.Run(() => document.Save());
            documentTask.Wait();
        }
    }

    public class FileInfoDisplayName
    {
        public string DisplayName { get; set; }

        public FileInfo File { get; set; }
    }
}