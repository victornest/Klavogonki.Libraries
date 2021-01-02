using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class VocsService : IVocsService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IStorageProvider<List<Voc>> dataProvider = new StorageProvider<List<Voc>>("db_vocs.dat");

        public static List<Voc> VocsDB { get; private set; }

        public VocsService()
        {
            VocsDB = dataProvider.Read();
        }

        public Voc GetVocById(int vocId)
        {
            Voc voc = VocsDB.Find(z => z.Id == vocId);
            return voc;
        }

        public Voc GetVocByName(string vocName)
        {
            Voc voc = VocsDB.Find(z => string.Equals(z.Name, vocName, StringComparison.OrdinalIgnoreCase));
            return voc;
        }

        public List<Voc> GetVocsByPattern(string vocNamePattern)
        {
            vocNamePattern = vocNamePattern.ToLower();
            var vocs = VocsDB.Where(z => z.Name.ToLower().Contains(vocNamePattern)).OrderByDescending(x => x.MarksCnt >= 50).ThenByDescending(x => x.Name.ToLower().StartsWith(vocNamePattern)).Take(15);
            return vocs.ToList();
        }


        public async Task UpdateList()
        {
            ChangeProgress(new Progress(0));
            List<Voc> Vocs = new List<Voc>();
            int counter = 1;

            for (var pageCnt = 1; pageCnt < 1500; pageCnt++)
            {
                ChangeProgress(new Progress(pageCnt, 0, Vocs.Count));
                string page = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/vocs/top/" + pageCnt);
                if (page.Contains("<p>Совпадений не найдено.</p>")) break;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);
                var table = doc.DocumentNode.SelectSingleNode("//div[@class='content-col']/table");
                if (table == null) break;
                var _vocs = table.SelectNodes("tr");  ////  
                foreach (var v in _vocs)
                {
                    Voc voc = new Voc();
                    voc.Position = counter++;
                    var tdTitle = v.SelectSingleNode("td[@class='title']");
                    var _name = tdTitle.SelectSingleNode("a");
                    voc.Name = _name.InnerText.Replace("\t", "   ");
                    if (voc.Name.Length > 0 && voc.Name[0] == '"') voc.Name = " " + voc.Name;
                    voc.Id = int.Parse(Regex.Match(_name.Attributes["href"].Value, "\\d+").Value);
                    voc.Description = tdTitle.SelectSingleNode("div[@class='desc']").InnerText.Replace("\r", " ").Replace("\n", "  ").Replace("\t", "   ");
                    if (voc.Description.Length > 0 && voc.Description[0] == '"') voc.Description = " " + voc.Description;
                    var _author = tdTitle.SelectSingleNode("div[@class='author']/a");
                    voc.AuthorId = int.Parse(Regex.Match(_author.Attributes["href"].Value, "\\d+").Value);
                    voc.AuthorNick = _author.InnerText;
                    voc.MarksCnt = int.Parse(Regex.Match(tdTitle.SelectSingleNode("span").InnerText, "\\d+").Value);

                    var tdSymbols = v.SelectSingleNode("td[@class='symbols']");
                    voc.Type = tdSymbols.SelectSingleNode("strong").InnerText.Trim();

                    string _symbols = tdSymbols.InnerText;
                    int.TryParse(Regex.Match(_symbols, "\\d+(?=&nbsp;отрыв)").Value, out int snippets);
                    int.TryParse(Regex.Match(_symbols, "\\d+(?=&nbsp;текст)").Value, out int texts);
                    int.TryParse(Regex.Match(_symbols, "\\d+(?=&nbsp;фраз)").Value, out int phrases);
                    int.TryParse(Regex.Match(_symbols, "\\d+(?=&nbsp;слов)").Value, out int words);
                    if (int.TryParse(Regex.Match(_symbols, "\\d+(?=&nbsp;символ)").Value, out int symbolsCnt)) voc.SymbolsCnt = symbolsCnt;

                    int first = snippets;
                    if (first == 0) first = texts;
                    if (first == 0) first = phrases;
                    if (first == 0) first = words;
                    voc.SnippetsCnt = first;
                    Vocs.Add(voc);
                }
                VocsDB = Vocs;
                dataProvider.Save(VocsDB);
                ChangeProgress(new Progress(pageCnt, pageCnt, Vocs.Count));
            }
        }

        public async Task<string> GetVocsLanguage(string[] vocIds)
        {
            //http://klavogonki.ru/vocs/115208
            StringBuilder sb = new StringBuilder("voc id\ten chars\tlat.diacr\tru chars\tcyr\tponct\tother\ttotal\r\n", 512);
            for (int i = 0; i < vocIds.Length; i++)
            {
                ChangeProgress(new Progress(i, vocIds.Length));
                if (vocIds[i] == "") continue;
                string page = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/vocs/" + vocIds[i]);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                doc.LoadHtml(page);

                int ru = 0;
                int en = 0;
                int latdiacr = 0;
                int cyr = 0;
                int total = 0;
                int ponctuation = 0;
                int other = 0;
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='words']");

                string chLat = "áàâäăāãåąæḃćċĉčçḋďḑđðéèėêëěēẽęəḟǵġĝğģǥḣĥḩħıíìİîïǐīįĳĵḱǩķĺľļłḿṁńṅňñņóòôöǒōõǫőøœṕṗŕřŗśṡŝšşșßṫťţțþŧúùûüŭūůųűṽẃẁẇŵẅẋẍýỳẏŷÿỹźżẑžƶ";
                string chCyr = "ѓғґђѐәєӂҗҙѕѝӣіїјҡќқљңһњөҫћӯүұҳҷџ";
                string chPonct = " \r\n\t№,.<>\\{\\}!@#$%\\^&*()_+=;:'\"/?\\|\\\\1234567890–—/;\\[\\]^`~¡¦¨¯´¿ˇ˙˚˛˝‘’‚‛“”„‟¢£¤¥€∙±«»×÷≈↑→↓←©¬®°¶…✓✔‰¹²³∞ª-";
                string str = "";
                if (node != null)
                {
                    string fragm1 = node.InnerText.ToLower();
                    //fragm1 = "Kādā rāmā un dzidrā vakarā aprīļa beigās Susuriņš nāca no tik tāliem ziemeļiem, ka ari pats uz savas ziemeļpuses nesa sniega pārslas.".ToLower();
                    ru = fragm1.Count(x => Regex.IsMatch(x.ToString(), "[а-яё]"));
                    en = fragm1.Count(x => Regex.IsMatch(x.ToString(), "[a-z]"));
                    latdiacr = fragm1.Count(x => Regex.IsMatch(x.ToString().ToLower(),
                    "[" + chLat + "]"));
                    cyr = fragm1.Count(x => Regex.IsMatch(x.ToString(), "[" + chCyr + "]"));
                    ponctuation = fragm1.Count(x => Regex.IsMatch(x.ToString(), "[" + chPonct + "]"));
                    var pattern = "[a-zа-яё" + chLat + chCyr + chPonct + "]";
                    other = fragm1.Count(x => !Regex.IsMatch(x.ToString(), pattern));
                    total = fragm1.Length;
                    str = new string(fragm1.Where(x => !Regex.IsMatch(x.ToString(), pattern)).ToArray());
                }


                sb.App(vocIds[i]);
                sb.App(en);
                sb.App(latdiacr);
                sb.App(ru);
                sb.App(cyr);
                sb.App(ponctuation);
                sb.App(other);
                sb.App(total);
                sb.App(str);
                sb.AppendLine();
                if (i == vocIds.Length - 1 || i > 0 && i % 500 == 0)
                {
                    File.WriteAllText("books.txt", sb.ToString(), Encoding.UTF8);
                }
            }
            ChangeProgress(new Progress(vocIds.Length, vocIds.Length));
            File.WriteAllText("vocs_languages.txt", sb.ToString(), Encoding.UTF8);
            return sb.ToString();
        }

        public async Task<string> GetVocsPopularity(string text)
        {
            StringBuilder sb = new StringBuilder("Оценок\tИспольз\tНазвание и id\r\n");
            var matches = Regex.Matches(text, "(?<=/vocs/)\\d+(?=/?)");
            int counter = 0;
            foreach (var id in matches)
            {
                ChangeProgress(new Progress(counter++, matches.Count));
                string page = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/vocs/" + id + "/history/");
                var mtch = Regex.Match(page, "<title>(.+) - Словари - Клавогонки(?:.|\n)+rating_cnt\">(\\d+)<(?:.|\n)+fav_cnt'>(\\d+)<");
                var name = mtch.Groups[1];
                var mark = mtch.Groups[2];
                var inuse = mtch.Groups[3];
                sb.Append(mark + "\t" + inuse + "\t" + name + "  (id " + id + ")\r\n");
            }
            ChangeProgress(new Progress(matches.Count, matches.Count));
            return sb.ToString();
        }

        public async Task<string> GetVocsPopularityAndSort(string[] lines)
        {
            List<Voc> listVoc = new List<Voc>();
            for (int i = 0; i < lines.Length; i++)
            {
                ChangeProgress(new Progress(i, lines.Length));
                int id = int.Parse(Regex.Match(lines[i], "(?<=/vocs/)\\d+(?=/?)").Value);
                try
                {
                    string page = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/vocs/" + id + "/history/");
                    var mtch = Regex.Match(page, "<title>(.+) - Словари - Клавогонки(?:.|\n)+rating_cnt\">(\\d+)<(?:.|\n)+fav_cnt'>(\\d+)<");
                    string name = mtch.Groups[1].ToString();
                    int marks = int.Parse(mtch.Groups[2].ToString());
                    int inuse = int.Parse(mtch.Groups[3].ToString());
                    Voc voc = new Voc() { Text = lines[i], Id = id, Name = name, InUseCnt = inuse, MarksCnt = marks };
                    listVoc.Add(voc);
                }
                catch (Exception)
                {
                }
            }
            listVoc.Sort();
            StringBuilder sb = new StringBuilder();
            foreach (Voc voc in listVoc)
            {
                sb.Append(voc.Text);
                sb.Append("\r\n");
            }
            ChangeProgress(new Progress(lines.Length, lines.Length));
            return sb.ToString();
        }

    }
}
