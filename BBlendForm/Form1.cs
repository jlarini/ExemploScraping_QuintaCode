using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
using Scraping.Web;

namespace BBlendForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        HtmlAgilityPack.HtmlNodeCollection lista;
        int posicao = 0;
        
        private void button1_Click(object sender, EventArgs e)
        {
            posicao++;
            PreencherTela();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            posicao--;
            PreencherTela();
        }

        public void PreencherTela()
        {
            var item = lista[posicao];

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(item.OuterHtml);

            string nome = doc.GetByClassNameEquals("nome").FirstOrDefault().InnerText;
            lblProduto.Text = nome;

            var detalhes = doc.GetByClassNameEquals("price").FirstOrDefault().InnerHtml;

            lblPreco.Text = detalhes.GetByClassNameEquals("de").FirstOrDefault()?.InnerHtml?.GetByClassNameEquals("val")?.FirstOrDefault()?.InnerText.Trim();
            lblPrecoPor.Text = detalhes.GetByClassNameEquals("por").FirstOrDefault().InnerHtml.GetByClassNameEquals("val").FirstOrDefault().InnerText.Trim();

            var imagens = doc.ParseImage();
            string urlImagem = imagens.FirstOrDefault().Src;

            pictureBox1.Load(urlImagem);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string url = "https://loja.bblend.com.br/bebidas?O=OrderByScoreAsc";

            var ret = new HttpRequestFluent(true)
               .FromUrl(url)
               .TryGetComponents(Enums.TypeComponent.Image
               | Enums.TypeComponent.ComboBox
               | Enums.TypeComponent.DataGrid
               | Enums.TypeComponent.InputHidden
               | Enums.TypeComponent.InputText
               | Enums.TypeComponent.LinkButton)
               .Load();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc.LoadHtml(ret.HtmlPage);
            lista = doc.GetByClassNameContains("detalhes");
            PreencherTela();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var item = lista[posicao];
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(item.OuterHtml);

            var imagens = doc.ParseImage();
            string urlImagem = imagens.FirstOrDefault().Src;
            txtUrl.Text = urlImagem;

            Process.Start("chrome.exe", urlImagem);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var http = new HttpRequestFluent(true);
            http.OnLoad += Http_OnLoad;
            
            var info = http
                .TryGetComponents(Enums.TypeComponent.InputHidden | Enums.TypeComponent.InputText)
                .AddHeaderDynamically(false)
                .FromUrl("https://github.com/login")
                .Load();

            NameValueCollection parametros = new NameValueCollection();

            string token = info.Components.InputHidden.FirstOrDefault(d => d.Name == "authenticity_token").Text;
            string timestamp = info.Components.InputHidden.FirstOrDefault(d => d.Name == "timestamp").Text;
            string timestampSecret = info.Components.InputHidden.FirstOrDefault(d => d.Name == "timestamp_secret").Text;
            string field = info.Components.InputTexts.FirstOrDefault(d => d.Name.Contains("required_field")).Name;

            parametros.Add("commit", "Sign in");
            parametros.Add("authenticity_token", token);
            parametros.Add("ga_id", "");
            parametros.Add("login", "SEU_EMAIL");
            parametros.Add("password", ConfigurationManager.AppSettings["senha"]);
            parametros.Add("webauthn-support", "supported");
            parametros.Add("webauthn-iuvpaa-support", "unsupported");
            parametros.Add("return_to", "");
            parametros.Add("allow_signup", "");
            parametros.Add("client_id", "");
            parametros.Add("integration", "");
            parametros.Add(field, "");
            parametros.Add("timestamp", timestamp);
            parametros.Add("timestamp_secret", timestampSecret);

            //efetuar login
            var posLogin =
                http.AddHeaderDynamically(true)
                .WithAutoRedirect(true)
                .WithParameters(parametros)
                .FromUrl("https://github.com/session")
                .Load();

        }

        private void Http_OnLoad(object sender, RequestHttpEventArgs e)
        {
            
        }
    }
}
