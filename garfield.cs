// windows.forms without vs designer challenge
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
public class Garfield : Form{
	public class Comic{
		public Comic(string name, DateTime minDate, DateTime maxDate, string urlFormat, string fileName){
			this.name = name;
			this.minDate = minDate;
			this.maxDate = maxDate;
			this.urlFormat = urlFormat;
			this.fileName = fileName;
		}
		[JsonConstructor]
		public Comic(string name, string minDate, string maxDate, string urlFormat, string fileName){
			this.name = name;
			this.minDate = DateTime.Parse(minDate);
			this.maxDate = DateTime.Parse(maxDate ?? DateTime.Now.ToString());
			this.urlFormat = urlFormat;
			this.fileName = fileName;
		}
		public string name {get; set;}
		public DateTime minDate {get; set;}
		public DateTime maxDate {get; set;}
		public string urlFormat {get; set;}
		public string fileName {get; set;}
	}
	public TableLayoutPanel panel;
	public TableLayoutPanel picker;
	public Button previous;
	public DateTimePicker date;
	public Button next;
	public PictureBox strip;
	public WebClient stripretriever;
	public ContextMenuStrip stripmenu;
	public StatusStrip status;
	public ToolStripStatusLabel statuscomic;
	public ToolStripStatusLabel statusdate;
	public MenuStrip menu;
	public List<Comic> comics;
	public Comic currentcomic;
	public string[] taglines = new string[] {
		"now with 15% more C#!", 
		"just like the web verison, but standalone!", 
		"featuring U.S. Acres!",
		"now with 50% less WinForms Designer!",
		"part of the WinForms without Designer challenge!",
		"now with 50% more random taglines!",
		"from the same author of HTML5 Strong Sad's Lament!",
		"because garfield.com was shot dead!",
		"watch Wade Duck tear a tag off of a pillow!",
		"because I can!",
		"now with 75% more CSC!",
		"featuring shitty code!"
	};

	public Garfield(){
		try{
			string json = File.ReadAllText(@"strips.json");
			comics = JsonConvert.DeserializeObject<List<Comic>>(json);
		}
		catch(Exception suck){
			MessageBox.Show(String.Format("Your strips.json is wrong.\n\n{0}\n\n...but I'll let you pass this time.", suck.ToString()), "UH OH IO!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			comics = JsonConvert.DeserializeObject<List<Comic>>(@"[
				{
					'name': 'Garfield',
					'minDate': '1978-06-19',
					'maxDate': '2020-07-22',
					'urlFormat': 'https://d1ejxu6vysztl5.cloudfront.net/comics/garfield/{0:yyyy}/{0:yyyy-MM-dd}.gif',
					'fileName': '{0:yyyy-MM-dd}.gif'
				},
				{
					'name': 'U.S. Acres',
					'minDate': '1986-03-03',
					'maxDate': '1989-05-07',
					'urlFormat': 'https://d1ejxu6vysztl5.cloudfront.net/comics/usacres/{0:yyyy}/usa{0:yyyy-MM-dd}.gif',
					'fileName': 'usa{0:yyyy-MM-dd}.gif'
				}
			]");
		}
		currentcomic = comics[0];
		this.AutoSize = true;
		this.MinimumSize = new Size(661,480);
		this.Text = @"Garfield strip picker - "+taglines[new Random().Next(0,taglines.Length)];
		stripretriever = new WebClient();
		stripretriever.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
		panel = new TableLayoutPanel();
		panel.ColumnCount = 0;
		panel.RowCount = 2;
		panel.Dock = DockStyle.Fill;
		panel.RowStyles.Clear();
		panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
		panel.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
		this.Controls.Add(panel);
		picker = new TableLayoutPanel();
		picker.ColumnCount = 3;
		picker.RowCount = 0;
		picker.Dock = DockStyle.Fill;
		picker.AutoSize = true;
		picker.ColumnStyles.Clear();
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90));
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
		panel.Controls.Add(picker);
		stripmenu = new ContextMenuStrip();
		stripmenu.Opening += new System.ComponentModel.CancelEventHandler(strip_menu);
		previous = new Button();
		previous.Dock = DockStyle.Fill;
		previous.Text = "Previous";
		previous.Click += new EventHandler(strip_previous);
		previous.Anchor = AnchorStyles.Left;
		picker.Controls.Add(previous);
		date = new DateTimePicker();
		date.MinDate = currentcomic.minDate;
		date.MaxDate = currentcomic.maxDate;
		date.CustomFormat = "yyyy-MM-dd";
		date.Format = DateTimePickerFormat.Custom;
		date.Dock = DockStyle.Fill;
		date.Anchor = AnchorStyles.None;
		date.ValueChanged+=new EventHandler(strip_update);
		picker.Controls.Add(date);
		next = new Button();
		next.Dock = DockStyle.Fill;
		next.Text = "Next";
		next.Click += new EventHandler(strip_next);
		next.Anchor = AnchorStyles.Right;
		picker.Controls.Add(next);
		strip = new PictureBox();
		strip.SizeMode = PictureBoxSizeMode.Zoom;
		strip.Dock = DockStyle.Fill;
		strip.ContextMenuStrip = stripmenu;
		strip.MinimumSize = new Size(640,0);
		panel.Controls.Add(strip);
		status = new StatusStrip();
		statuscomic = new ToolStripStatusLabel(currentcomic.name);
		statusdate = new ToolStripStatusLabel();
		status.Items.AddRange(new System.Windows.Forms.ToolStripItem[]{
			statuscomic,
			statusdate
		});
		this.Controls.Add(status);
		menu = new MenuStrip();
		ToolStripMenuItem file = new ToolStripMenuItem("&File");
		ToolStripMenuItem comic = new ToolStripMenuItem("&Comic");
		ToolStripMenuItem change = new ToolStripMenuItem("&Change comic");

		ToolStripMenuItem save = new ToolStripMenuItem("&Save strip", null, new EventHandler(strip_save), ( Keys.Control | Keys.S ));
		ToolStripMenuItem copy = new ToolStripMenuItem("&Copy strip image to clipboard", null, new EventHandler(strip_copy), ( Keys.Control | Keys.C ));
		ToolStripMenuItem copyURL = new ToolStripMenuItem("Copy strip &URL to clipboard", null, new EventHandler(strip_copyURL), ( Keys.Control | Keys.Alt | Keys.C ));

		ToolStripMenuItem exit = new ToolStripMenuItem("&Exit", null, new EventHandler(delegate(object sender, EventArgs e){ this.Close(); }), ( Keys.Alt | Keys.F4 ));
		ToolStripMenuItem nextstrip = new ToolStripMenuItem("&Next strip", null, new EventHandler(strip_next));
		ToolStripMenuItem previousstrip = new ToolStripMenuItem("&Previous strip", null, new EventHandler(strip_previous));
		file.DropDownItems.AddRange(new ToolStripMenuItem[]{
			save,
			copy,
			copyURL,
			exit
		});
		comic.DropDownItems.AddRange(new ToolStripMenuItem[]{
			change,
			nextstrip,
			previousstrip
		});
		for(int i = 0; i < comics.Count; i++){
			// im so fucked up
			Comic item = comics[i];
			change.DropDownItems.Add(new ToolStripMenuItem(item.name, null, new EventHandler((sender, e) => comic_update(sender, e, item))));
		}
		menu.Items.AddRange(new ToolStripItem[]{
			file,
			comic
		});
		this.Controls.Add(menu);
		strip_update(null, null);
	}
	private void strip_previous(object sender, EventArgs e){
		try{
			date.Value = date.Value.AddDays(-1);
		}
		catch(ArgumentOutOfRangeException suck){
			//we dun care if argumentoutofrangeexception
		}
	}
	private void strip_next(object sender, EventArgs e){
		try{
			date.Value = date.Value.AddDays(1);
		}
		catch(ArgumentOutOfRangeException suck){
			//we dun care if argumentoutofrangeexception
		}
	}
	private void strip_save(object sender, EventArgs e){
		SaveFileDialog savefile = new SaveFileDialog();
		savefile.InitialDirectory = @"C:\";
		savefile.DefaultExt = "gif";
		savefile.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
		savefile.Title = "Save comic strip";
		savefile.FileName = String.Format(currentcomic.fileName,date.Value);
		if(savefile.ShowDialog(this) == DialogResult.OK){
			System.IO.FileStream fs = (System.IO.FileStream)savefile.OpenFile();
			strip.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Gif);
		}
	}
	private void strip_copy(object sender, EventArgs e){
		Clipboard.SetImage(strip.Image);
	}
	private void strip_copyURL(object sender, EventArgs e){
		Clipboard.SetData(DataFormats.Text, (Object)String.Format(currentcomic.urlFormat, date.Value));
	}
	private void strip_menu(object sender, EventArgs e){
		ToolStripMenuItem save = new ToolStripMenuItem("&Save strip", null, new EventHandler(strip_save));
		ToolStripMenuItem copy = new ToolStripMenuItem("&Copy strip image to clipboard", null, new EventHandler(strip_copy));
		ToolStripMenuItem copyURL = new ToolStripMenuItem("Copy strip &URL to clipboard", null, new EventHandler(strip_copyURL));
		stripmenu.Items.Clear();
		stripmenu.Items.Add(save);
		stripmenu.Items.Add(copy);
		stripmenu.Items.Add(copyURL);
	}
	private void strip_update(object sender, EventArgs e){
		try{
		Stream stream = stripretriever.OpenRead(String.Format(currentcomic.urlFormat, date.Value));
		Image img = Image.FromStream(stream);
		strip.Image = img;
		}
		catch(WebException ex){
			Image img = new Bitmap(1200,350);
			Graphics graph = Graphics.FromImage(img);
			graph.Clear(Color.Gray);
			StringFormat idiot = new StringFormat();
			idiot.Alignment = StringAlignment.Center;
			idiot.LineAlignment = StringAlignment.Center;
			graph.DrawString(((int)((HttpWebResponse)ex.Response).StatusCode).ToString(), new Font("Arial", 96), new SolidBrush(Color.FromArgb(48,48,48)), new PointF(600,175), idiot);
			strip.Image = img;
		}
		statusdate.Text = date.Value.ToString("d");
	}

	private void comic_update(object sender, EventArgs e, Comic comic){
		currentcomic = comic;
		date.ResetBindings();
		date.Checked = true;
		date.MinDate = currentcomic.minDate;
		date.MaxDate = currentcomic.maxDate;
		date.Value = date.Value;
		statuscomic.Text = currentcomic.name;
		strip_update(null, null);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData){
		switch(keyData){
			case Keys.Right:
				strip_next(null, null);
				return true;
			case Keys.Left:
				strip_previous(null, null);
				return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	[STAThread]
	public static void Main(){
		Application.EnableVisualStyles();
		Application.Run(new Garfield());
	}
}