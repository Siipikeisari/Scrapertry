using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlDocument = System.Windows.Forms.HtmlDocument;



namespace LyricsToText
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			textBox1.AppendText("Skriv in sidans namn");


		}

		public void form_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";
		}

		public void button1_Click(object sender, EventArgs e)
		{
			var timer = new Stopwatch();
			
			var path = @"c:\sida.txt";

			var artist = textBox1.Text;

			if (artist.Contains(" "))
			{
				artist.Replace(" ", "_");
			}

			var firstLetter = artist.Substring(0, 1);

			string httpUrl = @"http://www.azlyrics.com/" + firstLetter + @"/" + artist + @".html";

			var page = GetWebPageHtmlFromUrl(httpUrl);

			File.WriteAllText(path, page);

			timer.Start();

			GetSongLinkFromHtml(path, artist);

			timer.Stop();

		}
		//hämtar sidan
		public static string GetWebPageHtmlFromUrl(string url)
		{
			var client = new HtmlWeb();
			HtmlAgilityPack.HtmlDocument doc = client.Load(url);
			return doc.DocumentNode.OuterHtml;
		}

		//hämtar ut alla låtlänkar formatterar om dom, samt kallar på metoden som ska ta ut texten
		public static void GetSongLinkFromHtml(string path, string name)
		{
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.Load(path);

			var hrefList = doc.DocumentNode.SelectNodes("//a")
							 .Select(p => p.GetAttributeValue("href", "not found")).Where(s => s.Contains(@"../lyrics"))
							 .ToList();

			var lyricLinks = hrefList.Select(line => line.Replace(@"../lyrics/", @"http://www.azlyrics.com/lyrics/")).Select(GetLyrics).ToList();

			foreach (var lyric in lyricLinks)
			{
				WriteLyricsToFile(lyric,name);
			}
		}

		//hämta låtsidan
		public static string GetLyrics(string url)
		{
			var client = new HtmlWeb();

			HtmlAgilityPack.HtmlDocument doc = client.Load(url);
			var page = doc.DocumentNode.InnerText;
			var newPage =RegexFormatter(page);

			return newPage;

		}

		//sök igenom sidan och ta ut texten
		public static string RegexFormatter(string page)
		{
			const string patternKey = "<!-- Usage of azlyrics.com content by any third-party lyrics provider is prohibited by our licensing agreement. Sorry about that. -->";
			const string pattern = patternKey + "(?s)(.*)Submit Corrections";

			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

			var formatPage = page.Replace(@"/n", " ");

			Match match = regex.Match(formatPage);

			return match.ToString();
		}

		static void WriteLyricsToFile(string lyrics, string name)
		{
			string localFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			using (var outPut = new StreamWriter(localFilePath + @"\" + name + ".txt",true))
			{
				outPut.WriteLine(lyrics);
			}
		}
	}

}
