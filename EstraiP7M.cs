using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EstraiP7MApp
{
    public class MainForm : Form
    {
        private TextBox txtFolder;
        private Button btnBrowse;
        private Button btnExtract;
        private RichTextBox txtLog;
        private Label lblStatus;

        public MainForm()
        {
            this.Text = "Estrattore Multiplo File P7M";
            this.Size = new Size(560, 430);
            this.MinimumSize = new Size(500, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f);

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            // Pannello intestazione
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(240, 244, 248),
                Padding = new Padding(15, 12, 15, 10)
            };

            Label lblTitle = new Label
            {
                Text = "Estrazione massiva di file firmati digitalmente (.p7m)",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 50, 90),
                AutoSize = true,
                Location = new Point(15, 12)
            };

            Label lblDesc = new Label
            {
                Text = "Seleziona la cartella contenente i file .p7m. Il programma estrarrà automaticamente\n" +
                       "i documenti originali (anche con firme multiple nidificate o codifica Base64)\n" +
                       "salvandoli nella sottocartella \"estratti\".",
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = true,
                Location = new Point(15, 36)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblDesc);

            // Selezione cartella
            Label lblDir = new Label
            {
                Text = "Cartella contenente i file .p7m:",
                Location = new Point(15, 100),
                AutoSize = true
            };

            txtFolder = new TextBox
            {
                Location = new Point(15, 122),
                Width = 420,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnBrowse = new Button
            {
                Text = "Sfoglia...",
                Location = new Point(445, 120),
                Width = 85,
                Height = 27,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowse.Click += BtnBrowse_Click;

            // Pulsante Avvia
            btnExtract = new Button
            {
                Text = "Avvia Estrazione",
                Location = new Point(15, 160),
                Width = 515,
                Height = 36,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnExtract.FlatAppearance.BorderSize = 0;
            btnExtract.Click += BtnExtract_Click;

            // Riquadro log
            txtLog = new RichTextBox
            {
                Location = new Point(15, 210),
                Size = new Size(515, 145),
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9f),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Barra stato
            lblStatus = new Label
            {
                Text = "Pronto.",
                Location = new Point(15, 362),
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblDir);
            this.Controls.Add(txtFolder);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(btnExtract);
            this.Controls.Add(txtLog);
            this.Controls.Add(lblStatus);

            // Se avviato passando una cartella o un file come argomento (es. trascinamento o Invia a)
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (Directory.Exists(args[1]))
                    txtFolder.Text = args[1];
                else if (File.Exists(args[1]))
                    txtFolder.Text = Path.GetDirectoryName(args[1]);
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Seleziona la cartella con i file .p7m da estrarre";
                if (!string.IsNullOrWhiteSpace(txtFolder.Text) && Directory.Exists(txtFolder.Text))
                    fbd.SelectedPath = txtFolder.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void AppendLog(string message, Color color)
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionColor = txtLog.ForeColor;
            txtLog.ScrollToCaret();
        }

        private static string GetStrippedFileName(string fileName)
        {
            string name = fileName;
            while (name.EndsWith(".p7m", StringComparison.OrdinalIgnoreCase))
            {
                name = Path.GetFileNameWithoutExtension(name);
            }
            return name;
        }

        private static string GetUniqueFilePath(string directory, string fileName, HashSet<string> sessionWrittenPaths)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            string candidate = Path.Combine(directory, fileName);
            int counter = 1;

            while (File.Exists(candidate) || sessionWrittenPaths.Contains(candidate))
            {
                candidate = Path.Combine(directory, string.Format("{0} ({1}){2}", baseName, counter, ext));
                counter++;
            }

            return candidate;
        }

        private void BtnExtract_Click(object sender, EventArgs e)
        {
            string sourceDir = txtFolder.Text.Trim();
            if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
            {
                MessageBox.Show("Seleziona una cartella valida prima di procedere.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtLog.Clear();
            string[] p7mFiles = Directory.GetFiles(sourceDir, "*.p7m", SearchOption.TopDirectoryOnly);

            if (p7mFiles.Length == 0)
            {
                AppendLog("Nessun file .p7m trovato nella cartella selezionata.", Color.DarkOrange);
                lblStatus.Text = "Nessun file elaborato.";
                return;
            }

            string outDir = Path.Combine(sourceDir, "estratti");
            bool overwriteExisting = false;

            // Verifica preventiva di eventuali file già presenti nella cartella di destinazione
            if (Directory.Exists(outDir))
            {
                bool hasConflicts = false;
                foreach (string filePath in p7mFiles)
                {
                    string expectedName = GetStrippedFileName(Path.GetFileName(filePath));
                    if (File.Exists(Path.Combine(outDir, expectedName)))
                    {
                        hasConflicts = true;
                        break;
                    }
                }

                if (hasConflicts)
                {
                    DialogResult choice = MessageBox.Show(
                        "Nella cartella \"estratti\" sono già presenti uno o più file con lo stesso nome.\n\n" +
                        "Vuoi sovrascrivere i file esistenti?\n" +
                        "• Sì: sostituisce i file già presenti.\n" +
                        "• No: mantiene i file esistenti e salva i nuovi con un suffisso automatico (es. documento (1).pdf).",
                        "File già esistenti nella destinazione",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (choice == DialogResult.Cancel)
                    {
                        lblStatus.Text = "Operazione annullata.";
                        return;
                    }

                    overwriteExisting = (choice == DialogResult.Yes);
                }
            }

            Directory.CreateDirectory(outDir);

            btnExtract.Enabled = false;
            int okCount = 0;
            int errCount = 0;
            HashSet<string> sessionWrittenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string filePath in p7mFiles)
            {
                string fileName = Path.GetFileName(filePath);
                try
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    string currentName = fileName;

                    while (currentName.EndsWith(".p7m", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            string ascii = Encoding.ASCII.GetString(data);
                            string cleaned = Regex.Replace(ascii, "-----[^-]+-----", "");
                            cleaned = Regex.Replace(cleaned, "\\s+", "");

                            if (cleaned.Length > 0 && Regex.IsMatch(cleaned, "^[A-Za-z0-9+/=]+$"))
                            {
                                data = Convert.FromBase64String(cleaned);
                            }
                        }
                        catch
                        {
                            // Non è Base64: prosegue con i byte binari originali
                        }

                        SignedCms cms = new SignedCms();
                        cms.Decode(data);
                        data = cms.ContentInfo.Content;
                        currentName = Path.GetFileNameWithoutExtension(currentName);
                    }

                    string destPath = Path.Combine(outDir, currentName);

                    // Se l'utente ha scelto di NON sovrascrivere, oppure se due P7M dello stesso lotto
                    // hanno generato lo stesso nome finale nella sessione corrente, assegna suffisso (1), (2)...
                    if (!overwriteExisting || sessionWrittenPaths.Contains(destPath))
                    {
                        destPath = GetUniqueFilePath(outDir, currentName, sessionWrittenPaths);
                    }

                    File.WriteAllBytes(destPath, data);
                    sessionWrittenPaths.Add(destPath);

                    string finalSavedName = Path.GetFileName(destPath);
                    AppendLog("[OK] Estratto: " + finalSavedName, Color.DarkGreen);
                    okCount++;
                }
                catch (Exception ex)
                {
                    AppendLog("[ERRORE] " + fileName + " -> " + ex.Message, Color.Red);
                    errCount++;
                }
                Application.DoEvents();
            }

            btnExtract.Enabled = true;
            lblStatus.Text = string.Format("Completato: {0} estratti, {1} errori.", okCount, errCount);

            if (okCount > 0)
            {
                DialogResult res = MessageBox.Show(
                    string.Format("Estrazione completata ({0} file estratti).\nVuoi aprire la cartella \"estratti\"?", okCount),
                    "Operazione completata",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (res == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", outDir);
                }
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}