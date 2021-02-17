// windows.forms without vs designer challenge
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
public class Garfield : Form
{
	public class Comic
	{
		public Comic(string name, DateTime minDate, DateTime maxDate, string urlFormat, string fileName, int dayIncrement)
		{
			this.name = name;
			this.minDate = minDate;
			this.maxDate = maxDate;
			this.urlFormat = urlFormat;
			this.fileName = fileName;
		}
		[JsonConstructor]
		public Comic(string name, string minDate, string maxDate, string urlFormat, string fileName, int dayIncrement, int? weekday)
		{
			this.name = name;
			this.minDate = DateTime.Parse(minDate);
			this.maxDate = DateTime.Parse(maxDate ?? DateTime.Now.ToString());
			this.urlFormat = urlFormat;
			this.fileName = fileName;
			this.dayIncrement = dayIncrement;
			this.weekday = weekday;
		}
		public string name { get; set; }
		public DateTime minDate { get; set; }
		public DateTime maxDate { get; set; }
		public string urlFormat { get; set; }
		public string fileName { get; set; }

		[DefaultValue(1)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public int dayIncrement { get; set; }

		[DefaultValue(null)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? weekday { get; set; }
	}
	public class Gimmicks
	{
		public Gimmicks()
		{
			this.twopanel = false;
			this.pipe = false;
		}
		public bool AtLeastOne(){
			PropertyInfo[] piss = this.GetType().GetProperties();
			foreach(PropertyInfo prop in piss){
				bool val = (bool)prop.GetValue(this);
				if(val){
					return true;
				}
			}
			return false;
		}
		public bool twopanel { get; set; }
		public bool pipe { get; set; }
	}
	public TableLayoutPanel panel;
	public TableLayoutPanel picker;
	public Button previous;
	public DateTimePicker date;
	public Button next;
	public PictureBox strip;
	public static HttpClient stripretriever;
	public ContextMenuStrip stripmenu;
	public StatusStrip status;
	public ToolStripStatusLabel statuscomic;
	public ToolStripStatusLabel statusdate;
	public ToolStripProgressBar statusprogress;
	public MenuStrip menu;
	public List<Comic> comics;
	public Comic currentcomic;
	public Gimmicks gimmicks;
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
	private CancellationTokenSource ctk = new CancellationTokenSource();
	public Bitmap shittyCopy(Image bitmap){
		Bitmap bm = new Bitmap(bitmap.Width, bitmap.Height);
		using(Graphics g = Graphics.FromImage(bm)){
			g.DrawImage(bitmap, 0, 0, new RectangleF(new Point(0, 0), new Size(bitmap.Width, bitmap.Height)), GraphicsUnit.Pixel);
		};
		return bm;
	}
	ToolStripMenuItem file;
	ToolStripMenuItem comic;
	ToolStripMenuItem gimmick;
	ToolStripMenuItem twopanel;
	ToolStripMenuItem pipe;
	ToolStripMenuItem change;
	ToolStripMenuItem save;
	ToolStripMenuItem copy;
	ToolStripMenuItem copyURL;
	ToolStripMenuItem nextstrip;
	ToolStripMenuItem previousstrip;
	ToolStripMenuItem gorando;
	ToolStripMenuItem exit;
	public Garfield()
	{
		try
		{
			string json = File.ReadAllText(@"strips.json");
			comics = JsonConvert.DeserializeObject<List<Comic>>(json);
		}
		catch (Exception suck)
		{
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
		file = new ToolStripMenuItem("&File");
		comic = new ToolStripMenuItem("&Comic");
		gimmick = new ToolStripMenuItem("&Gimmicks");
		twopanel = new ToolStripMenuItem("&Two panels", null, new EventHandler(strip_gimmick));
		twopanel.Tag = 0;
		pipe = new ToolStripMenuItem("&Pipe", null, new EventHandler(strip_gimmick));
		pipe.Tag = 1;
		change = new ToolStripMenuItem("&Change comic");
		save = new ToolStripMenuItem("&Save strip", null, new EventHandler(strip_save), (Keys.Control | Keys.S));
		copy = new ToolStripMenuItem("&Copy strip image to clipboard", null, new EventHandler(strip_copy), (Keys.Control | Keys.C));
		copyURL = new ToolStripMenuItem("Copy strip &URL to clipboard", null, new EventHandler(strip_copyURL), (Keys.Control | Keys.Shift | Keys.C));
		nextstrip = new ToolStripMenuItem("&Next strip", null, new EventHandler(strip_next));
		previousstrip = new ToolStripMenuItem("&Previous strip", null, new EventHandler(strip_previous));
		gorando = new ToolStripMenuItem("&Go rando", null, new EventHandler(strip_rando));
		exit = new ToolStripMenuItem("E&xit", null, new EventHandler(delegate (object sender, EventArgs e) { this.Close(); }), (Keys.Alt | Keys.F4));
		currentcomic = comics[0];
		this.AutoSize = true;
		this.MinimumSize = new Size(661, 480);
		this.Text = @"Garfield strip picker - " + taglines[new Random().Next(0, taglines.Length)];
		stripretriever = new HttpClient();
		System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		stripretriever.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
		// fuck you msdn you lied to me
		/*stripretriever.DownloadProgressChanged += new DownloadProgressChangedEventHandler(delegate (object sender, DownloadProgressChangedEventArgs e) {
			statusprogress.Value = e.ProgressPercentage;
		});*/
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
		stripmenu.Opening += new System.ComponentModel.CancelEventHandler(delegate (object sender, CancelEventArgs e) {
			stripmenu.Items.Clear();
			stripmenu.Items.AddRange(new ToolStripMenuItem[]{
				save,
				copy,
				copyURL,
				nextstrip,
				previousstrip,
				gorando
			});
		});
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
		date.ValueChanged += new EventHandler(strip_update);
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
		strip.MinimumSize = new Size(640, 0);
		panel.Controls.Add(strip);
		status = new StatusStrip();
		statuscomic = new ToolStripStatusLabel(currentcomic.name);
		statusdate = new ToolStripStatusLabel();
		statusprogress = new ToolStripProgressBar();
		statusprogress.Alignment = ToolStripItemAlignment.Right;
		statusprogress.Visible = false;
		status.Items.AddRange(new System.Windows.Forms.ToolStripItem[]{
			statuscomic,
			statusdate,
			statusprogress
		});
		this.Controls.Add(status);
		menu = new MenuStrip();
		file.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) {
			file.DropDownItems.Clear();
			file.DropDownItems.AddRange(new ToolStripMenuItem[]{
				save,
				copy,
				copyURL,
				exit
			});
		});
		comic.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) {
			comic.DropDownItems.Clear();
			comic.DropDownItems.AddRange(new ToolStripMenuItem[]{
				change,
				nextstrip,
				previousstrip,
				gorando
			});
		});
		gimmicks = new Gimmicks();
		gimmick.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) {
			gimmick.DropDownItems.Clear();
			gimmick.DropDownItems.AddRange(new ToolStripMenuItem[]{
				twopanel,
				pipe
			});
		});
		for (int i = 0; i < comics.Count; i++)
		{
			// im so fucked up
			Comic item = comics[i];
			ToolStripMenuItem fuck = new ToolStripMenuItem(item.name, null, new EventHandler((sender, e) => comic_update(sender, e)));
			fuck.Tag = item;
			change.DropDownItems.Add(fuck);
		}
		menu.Items.AddRange(new ToolStripItem[]{
			file,
			comic,
			gimmick
		});
		this.Controls.Add(menu);
		strip_rando(null, null);
		strip_update(null, null);
	}
	private void strip_previous(object sender, EventArgs e)
	{
		try
		{
			date.Value = date.Value.AddDays(-currentcomic.dayIncrement);
		}
		catch (ArgumentOutOfRangeException suck)
		{
			//we dun care if argumentoutofrangeexception
		}
	}
	private void strip_next(object sender, EventArgs e)
	{
		try
		{
			date.Value = date.Value.AddDays(currentcomic.dayIncrement);
		}
		catch (ArgumentOutOfRangeException suck)
		{
			//we dun care if argumentoutofrangeexception
		}
	}

	// never heard of this approach
	// this is interesting
	// https://stackoverflow.com/questions/194863/random-date-in-c-sharp

	private void strip_rando(object sender, EventArgs e)
	{
		try
		{
			int r = (currentcomic.maxDate - currentcomic.minDate).Days;
			DateTime rd = currentcomic.minDate.AddDays(new Random().Next(r));
			if(currentcomic.weekday is int butt){
				date.Value = rd.AddDays(7-((int)rd.DayOfWeek-butt%7));
			}
			else{
				date.Value = rd;
			}
		}
		catch (ArgumentOutOfRangeException suck)
		{
			//we dun care if argumentoutofrangeexception
		}
	}

	private void strip_save(object sender, EventArgs e)
	{
		SaveFileDialog savefile = new SaveFileDialog();
		savefile.InitialDirectory = @"C:\";
		savefile.DefaultExt = "gif";
		savefile.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
		savefile.Title = "Save comic strip";
		savefile.FileName = String.Format(currentcomic.fileName, date.Value);
		if (savefile.ShowDialog(this) == DialogResult.OK)
		{
			System.IO.FileStream fs = (System.IO.FileStream)savefile.OpenFile();
			strip.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Gif);
		}
	}

	private void strip_copy(object sender, EventArgs e)
	{
		Clipboard.SetImage(strip.Image);
	}

	private void strip_copyURL(object sender, EventArgs e)
	{
		Clipboard.SetData(DataFormats.Text, (Object)String.Format(currentcomic.urlFormat, date.Value));
	}

	private Image drawMessage(string message, int size = 96)
    {
		Image img = new Bitmap(1200, 350);
		Graphics graph = Graphics.FromImage(img);
		graph.Clear(Color.Gray);
		StringFormat idiot = new StringFormat();
		idiot.Alignment = StringAlignment.Center;
		idiot.LineAlignment = StringAlignment.Center;
		graph.DrawString(message, new Font("Arial", size), new SolidBrush(Color.FromArgb(48, 48, 48)), new PointF(600, 175), idiot);
		return img;
	}

	private async void strip_update(object sender, EventArgs e)
	{
		statusprogress.Visible = true;
		Uri fuck;
		bool good = Uri.TryCreate(String.Format(currentcomic.urlFormat, date.Value), UriKind.Absolute, out fuck);
		if(good == false){
			Uri.TryCreate(Path.GetFullPath(String.Format(currentcomic.urlFormat, date.Value)), UriKind.Absolute, out fuck);
		}
		//why in the world does absoluteuri return a string and not an uri
		/*
		had to break my balls for this one. 
		none of the uri class methods don't even fucking support
		relative paths.
		*/
		Image img;
		if(fuck.IsFile){
			string path = fuck.LocalPath;
			try{
				FileStream fs = await Task.Run(() => File.OpenRead(path));//File.OpenRead(path); its asynchronous now lol
				img = Image.FromStream(fs, false, false);
			}
			catch (FileNotFoundException suck)
			{
				img = drawMessage("not found");
			}
			catch(IOException suck)
	        {
				img = drawMessage("i/o exception.\nyour file must be locked.", 36);
			}
		}
		else{
			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				//Stream stream = stripretriever.OpenRead(String.Format(currentcomic.urlFormat, date.Value));
				//ctk.Cancel();
				response = await stripretriever.GetAsync(fuck, ctk.Token);
				response.EnsureSuccessStatusCode();
				Stream stream = await response.Content.ReadAsStreamAsync();
				img = Image.FromStream(stream, false, false);
			}
			catch (HttpRequestException suck)
			{
				//MessageBox.Show(suck.ToString());
				img = drawMessage(((int)(response.StatusCode)).ToString());
			}
			catch(ArgumentException suck)
			{
				img = drawMessage("an error occured while\nprocessing the image", 36);
			}
		}
		Bitmap bm = new Bitmap(img.Width, img.Height);
		using(bm){
			bm = this.shittyCopy(img);
			if(gimmicks.twopanel){
				int width = (int)(img.Width/1.5);
				bm = new Bitmap(width, img.Height);
				using(Graphics g = Graphics.FromImage(bm)){
					g.DrawImage(img, 0, 0, new RectangleF(new Point(0, 0), new Size(width,img.Height)), GraphicsUnit.Pixel);
				};
			}
			if(gimmicks.pipe){
				string path = "./resource/pipe.png"; //I don't want to pack it as a resource so its in a folder
				if(File.Exists(path)){
					using(Bitmap pipeunscale = new Bitmap(path)){
						int width = img.Width>=1195&&img.Width<=1205?pipeunscale.Width:(int)(img.Width/3);
						Bitmap pipebm = new Bitmap(pipeunscale, new Size(width, img.Height));
						using(Graphics g = Graphics.FromImage(bm)){
							g.DrawImage(bm, 0, 0, new RectangleF(new Point(0, 0), new Size(bm.Width,bm.Height)), GraphicsUnit.Pixel);
							g.DrawImage(pipebm, bm.Width-pipebm.Width, 0, new RectangleF(new Point(0, 0), new Size(pipebm.Width,pipebm.Height)), GraphicsUnit.Pixel);
						};
						pipebm.Dispose();
					}
				}
			}
			if(gimmicks.AtLeastOne()){
				img = bm;
			}
		}
		strip.Image = img;
		statusprogress.Visible = false;
		statusdate.Text = date.Value.ToString("d");
	}

	private void comic_update(object sender, EventArgs e)
	{
		ToolStripMenuItem _where = sender as ToolStripMenuItem;
		if(_where == null) return;
		currentcomic = _where.Tag as Comic;
		date.ResetBindings();
		date.Checked = true;
		//i keep getting argumentoutofrangeexceptions. lets try this
		date.MaxDate = DateTimePicker.MaximumDateTime;
		date.MinDate = DateTimePicker.MinimumDateTime;
		//reset mindate and maxdate values then set it again
		date.MaxDate = currentcomic.maxDate;
		date.MinDate = currentcomic.minDate;
		date.Value = date.Value;
		statuscomic.Text = currentcomic.name;
		strip_update(null, null);
	}

	private void strip_gimmick(object sender, EventArgs e){
		//i had a slightly better idea of doing this but csc hated it
		ToolStripMenuItem gimmickItem = sender as ToolStripMenuItem;
		if(gimmickItem==null) return; //wanted to add checks for tags too but i cant figure that out
		gimmickItem.Checked = !gimmickItem.Checked;
		switch((int)gimmickItem.Tag){
			case 0:
				gimmicks.twopanel = gimmickItem.Checked;
				break;
			case 1:
				gimmicks.pipe = gimmickItem.Checked;
				break;
			default:
				return;
		}
		strip_update(null, null);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		switch (keyData)
		{
			case Keys.Right:
				strip_next(null, null);
				return true;
			case Keys.Left:
				strip_previous(null, null);
				return true;
			case Keys.R:
				strip_rando(null, null);
				return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	[STAThread]
	public static void Main()
	{
		Application.EnableVisualStyles();
		Application.Run(new Garfield());
	}
}
