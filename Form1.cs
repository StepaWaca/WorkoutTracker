using Microsoft.EntityFrameworkCore;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    //private void Form1_Load(object sender, EventArgs e)
    //{
    //}

    public partial class Form1 : Form
    {
        private WorkoutContext _db;
        private Client? _currentClient;
        private WorkoutProgram? _selectedProgram;

        // Контролы
        private ComboBox cmbClient;
        private ListBox lstPrograms;
        private ListBox lstExercises;
        private ComboBox cmbExercise;
        private DataGridView dgvActivities;
        private DateTimePicker dtpDate;
        private Panel stickerPanel;
        private Label lblTotal;
        private TextBox txtProgramName, txtProgramType;
        private CheckBox chkProgramActive;

        public Form1()
        {
            _db = new WorkoutContext();
            _db.Database.EnsureCreated();
            this.Text = "Учет тренировок";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateControls();
            LoadClients();
        }

        private void CreateControls()
        {
            // Клиент
            Label lblClient = new Label() { Text = "Клиент:", Location = new Point(12, 15), Size = new Size(50, 25) };
            cmbClient = new ComboBox() { Location = new Point(70, 12), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbClient.SelectedIndexChanged += (s, e) => LoadData();

            Button btnAddClient = new Button() { Location = new Point(280, 10), Size = new Size(100, 30), Text = "Новый клиент" };
            btnAddClient.Click += (s, e) =>
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("ФИО клиента:", "Новый клиент", "");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _db.Clients.Add(new Client { FullName = name });
                    _db.SaveChanges();
                    LoadClients();
                }
            };

            // Программы
            GroupBox gbPrograms = new GroupBox() { Text = "Программы", Location = new Point(12, 50), Size = new Size(250, 400) };
            lstPrograms = new ListBox() { Location = new Point(10, 25), Size = new Size(230, 150) };
            lstPrograms.DisplayMember = "Name";
            lstPrograms.SelectedIndexChanged += (s, e) =>
            {
                _selectedProgram = lstPrograms.SelectedItem as WorkoutProgram;
                if (_selectedProgram != null)
                {
                    txtProgramName.Text = _selectedProgram.Name;
                    txtProgramType.Text = _selectedProgram.Type;
                    chkProgramActive.Checked = _selectedProgram.IsActive;

                    var exercises = _db.Exercises.Where(x => x.ProgramId == _selectedProgram.Id && x.ClientId == _currentClient!.Id).ToList();
                    lstExercises.DataSource = exercises;
                    lstExercises.DisplayMember = "Name";
                }
            };

            Button btnAddProgram = new Button() { Location = new Point(10, 185), Size = new Size(70, 30), Text = "Добавить" };
            Button btnEditProgram = new Button() { Location = new Point(85, 185), Size = new Size(70, 30), Text = "Изменить" };
            Button btnDelProgram = new Button() { Location = new Point(160, 185), Size = new Size(80, 30), Text = "Удалить" };

            btnAddProgram.Click += (s, e) => AddEditProgram(null);
            btnEditProgram.Click += (s, e) => AddEditProgram(_selectedProgram);
            btnDelProgram.Click += (s, e) =>
            {
                if (_selectedProgram != null && MessageBox.Show("Удалить программу?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _db.WorkoutPrograms.Remove(_selectedProgram);
                    _db.SaveChanges();
                    LoadData();
                }
            };

            // Детали программы
            GroupBox gbDetails = new GroupBox() { Text = "Детали", Location = new Point(10, 225), Size = new Size(230, 160) }
            ;
            txtProgramName = new TextBox() { Location = new Point(80, 25), Size = new Size(140, 25), ReadOnly = true };
            txtProgramType = new TextBox() { Location = new Point(80, 60), Size = new Size(140, 25), ReadOnly = true };
            chkProgramActive = new CheckBox() { Text = "Активна", Location = new Point(80, 95), Size = new Size(100, 25), Enabled = false };

            gbDetails.Controls.AddRange(new Control[] {
                new Label() { Text = "Название:", Location = new Point(10, 30) }, txtProgramName,
                new Label() { Text = "Тип:", Location = new Point(10, 65) }, txtProgramType, chkProgramActive
            });

            gbPrograms.Controls.AddRange(new Control[] { lstPrograms, btnAddProgram, btnEditProgram, btnDelProgram, gbDetails });

            // Упражнения
            GroupBox gbExercises = new GroupBox() { Text = "Упражнения", Location = new Point(270, 50), Size = new Size(250, 400) };
            lstExercises = new ListBox() { Location = new Point(10, 25), Size = new Size(230, 250) };
            lstExercises.DisplayMember = "Name";

            Button btnAddExercise = new Button() { Location = new Point(10, 285), Size = new Size(70, 30), Text = "Добавить" };
            Button btnDelExercise = new Button() { Location = new Point(85, 285), Size = new Size(70, 30), Text = "Удалить" };
            Button btnToggleActive = new Button() { Location = new Point(160, 285), Size = new Size(80, 30), Text = "Актив" };

            btnAddExercise.Click += (s, e) =>
            {
                if (_selectedProgram == null) { MessageBox.Show("Выберите программу"); return; }
                string name = Microsoft.VisualBasic.Interaction.InputBox("Название упражнения:", "Добавить", "");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _db.Exercises.Add(new Exercise { Name = name, ClientId = _currentClient!.Id, ProgramId = _selectedProgram.Id, IsActive = true });
                    _db.SaveChanges();
                    LoadData();
                }
            };

            btnDelExercise.Click += (s, e) =>
            {
                if (lstExercises.SelectedItem is Exercise ex)
                {
                    _db.Exercises.Remove(ex);
                    _db.SaveChanges();
                    LoadData();
                }
            };

            btnToggleActive.Click += (s, e) =>
            {
                if (lstExercises.SelectedItem is Exercise ex)
                {
                    ex.IsActive = !ex.IsActive;
                    _db.SaveChanges();
                    LoadData();
                }
            };

            gbExercises.Controls.AddRange(new Control[] { lstExercises, btnAddExercise, btnDelExercise, btnToggleActive });

            // Активности
            GroupBox gbActivities = new GroupBox() { Text = "Тренировки", Location = new Point(530, 50), Size = new Size(540, 400) };

            dgvActivities = new DataGridView()
            {
                Location = new Point(10, 80),
                Size = new Size(520, 250),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Label lblExercise = new Label() { Text = "Упражнение:", Location = new Point(10, 15), Size = new Size(80, 25) };
            cmbExercise = new ComboBox() { Location = new Point(95, 12), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            Button btnAddActivity = new Button() { Location = new Point(285, 10), Size = new Size(100, 30), Text = "Выполнить" };
            Button btnDelActivity = new Button()
            {
                Location = new Point(395, 10),
                Size = new Size(80, 30),
                Text = "Удалить"
            };


            Label lblDate = new Label() { Text = "Дата:", Location = new Point(10, 50), Size = new Size(80, 25) };
            dtpDate = new DateTimePicker() { Location = new Point(95, 47), Size = new Size(120, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            Button btnFilter = new Button() { Location = new Point(225, 45), Size = new Size(80, 30), Text = "Фильтр" };
            Button btnShowAll = new Button() { Location = new Point(315, 45), Size = new Size(80, 30), Text = "Все" };

            btnAddActivity.Click += (s, e) => ShowAddActivityDialog();
            btnDelActivity.Click += (s, e) =>
            {
                if (dgvActivities.SelectedRows.Count > 0)
                {
                    int id = (int)dgvActivities.SelectedRows[0].Cells["Id"].Value;
                    var act = _db.Activities.Find(id);
                    if (act != null) { _db.Activities.Remove(act); _db.SaveChanges(); LoadActivities(); UpdateStats(); }
                }
            };

            btnFilter.Click += (s, e) => FilterActivities();
            btnShowAll.Click += (s, e) => LoadActivities();

            gbActivities.Controls.AddRange(new Control[] { dgvActivities, lblExercise, cmbExercise, btnAddActivity, btnDelActivity, lblDate, dtpDate, btnFilter, btnShowAll });

            // Статистика
            GroupBox gbStats = new GroupBox() { Text = "Статистика", Location = new Point(12, 460), Size = new Size(1058, 100) };
            lblTotal = new Label() { Location = new Point(10, 25), Size = new Size(500, 25), Font = new Font("Arial", 10, FontStyle.Bold) };
            stickerPanel = new Panel() { Location = new Point(10, 55), Size = new Size(150, 35), BorderStyle = BorderStyle.FixedSingle };

            gbStats.Controls.AddRange(new Control[] { lblTotal, stickerPanel });

            this.Controls.AddRange(new Control[] { lblClient, cmbClient, btnAddClient, gbPrograms, gbExercises, gbActivities, gbStats });
        }

        private void AddEditProgram(WorkoutProgram? program)
        {
            Form dialog = new Form() { Size = new Size(350, 200), Text = program == null ? "Новая программа" : "Редактирование", StartPosition = FormStartPosition.CenterParent };

            TextBox txtName = new TextBox() { Location = new Point(120, 20), Size = new Size(180, 25) };
            TextBox txtType = new TextBox() { Location = new Point(120, 60), Size = new Size(180, 25) };
            CheckBox chkActive = new CheckBox() { Text = "Активна", Location = new Point(120, 100), Size = new Size(100, 25) };

            if (program != null)
            {
                txtName.Text = program.Name;
                txtType.Text = program.Type;
                chkActive.Checked = program.IsActive;
            }

            Button btnSave = new Button() { Text = "Сохранить", Location = new Point(120, 140), Size = new Size(100, 30) };
            btnSave.Click += (s, e) =>
            {
                if (program == null)
                {
                    program = new WorkoutProgram();
                    _db.WorkoutPrograms.Add(program);
                }
                program.Name = txtName.Text;
                program.Type = txtType.Text;
                program.IsActive = chkActive.Checked;
                program.ClientId = _currentClient!.Id;
                _db.SaveChanges();
                dialog.Close();
                LoadData();
            };

            dialog.Controls.AddRange(new Control[] {
                new Label() { Text = "Название:", Location = new Point(20, 25), Size = new Size(80, 25) }, txtName,
                new Label() { Text = "Тип:", Location = new Point(20, 65), Size = new Size(80, 25) }, txtType, chkActive, btnSave
            });
            dialog.ShowDialog();
        }

        private void ShowAddActivityDialog()
        {
            if (cmbExercise.SelectedItem == null)
            {
                MessageBox.Show("Выберите упражнение");
                return;
            }

            Exercise ex = (Exercise)cmbExercise.SelectedItem;
            DateTime date = dtpDate.Value.Date;
            int dayTotal = _db.Activities.Where(a => a.ClientId == _currentClient!.Id && a.Date.Date == date).Sum(a => a.Minutes);

            Form dialog = new Form()
            {
                Size = new Size(350, 250),
                Text = "Добавить тренировку",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };

            NumericUpDown num = new NumericUpDown()
            {
                Minimum = 1,
                Maximum = 1440,
                Value = 30,
                Location = new Point(120, 80),
                Size = new Size(180, 25)
            };

            TextBox notes = new TextBox()
            {
                Location = new Point(120, 120),
                Size = new Size(180, 60),
                Multiline = true
            };

            Button btnSave = new Button()
            {
                Text = "Сохранить",
                Location = new Point(120, 195),
                Size = new Size(100, 30)
            };


            btnSave.Click += (s, e) =>
            {
                if (dayTotal + num.Value > 1440)
                {
                    MessageBox.Show($"Лимит 1440 мин! Уже {dayTotal} мин");
                }
                else
                {
                    _db.Activities.Add(new Activity
                    {
                        Date = date,
                        Minutes = (int)num.Value,
                        Notes = notes.Text,
                        ExerciseId = ex.Id,
                        ClientId = _currentClient!.Id
                    });
                    _db.SaveChanges();
                    LoadActivities();
                    UpdateStats();
                    dialog.Close();
                }
            };

            Button btnCancel = new Button()
            {
                Text = "Отмена",
                Location = new Point(230, 195),
                Size = new Size(80, 30)
            };
            btnCancel.Click += (_, _) => dialog.Close();

            dialog.Controls.AddRange(new Control[] {
                new Label() { Text = "Упражнение:", Location = new Point(20, 25), Size = new Size(80, 25) },
                new Label() { Text = ex.Name, Location = new Point(120, 25), Size = new Size(180, 25) },
                new Label() { Text = "Минуты:", Location = new Point(20, 85), Size = new Size(80, 25) }, num,
                new Label() { Text = "Заметки:", Location = new Point(20, 125), Size = new Size(80, 25) }, notes,
                btnSave, btnCancel
            });
            dialog.ShowDialog();
        }

        private void LoadClients()
        {
            var clients = _db.Clients.ToList();
            cmbClient.DataSource = clients;
            cmbClient.DisplayMember = "FullName";
            if (!clients.Any())
            {
                _db.Clients.Add(new Client { FullName = "Иван Иванов" });
                _db.SaveChanges();
                LoadClients();
                return;
            }
            LoadData();
        }

        private void LoadData()
        {
            if (cmbClient.SelectedItem == null) return;
            _currentClient = (Client)cmbClient.SelectedItem;

            lstPrograms.DataSource = _db.WorkoutPrograms.Where(p => p.ClientId == _currentClient.Id).ToList();
            cmbExercise.DataSource = _db.Exercises.Where(e => e.ClientId == _currentClient.Id && e.IsActive).ToList();
            cmbExercise.DisplayMember = "Name";

            LoadActivities();
            UpdateStats();
        }

        private void LoadActivities()
        {
            var acts = _db.Activities
                .Include(a => a.Exercise)
                .Where(a => a.ClientId == _currentClient!.Id)
                .OrderByDescending(a => a.Date)
                .Select(a => new { a.Id, Упражнение = a.Exercise != null ? a.Exercise.Name : "", Дата = a.Date.ToString("dd.MM.yyyy"), a.Minutes, a.Notes })
                .ToList();
            dgvActivities.DataSource = acts;
        }

        private void FilterActivities()
        {
            var acts = _db.Activities
                .Include(a => a.Exercise)
                .Where(a => a.ClientId == _currentClient!.Id && a.Date.Date == dtpDate.Value.Date)
                .Select(a => new { a.Id, Упражнение = a.Exercise != null ? a.Exercise.Name : "", Дата = a.Date.ToString("dd.MM.yyyy"), a.Minutes, a.Notes })
                .ToList();
            dgvActivities.DataSource = acts;
            UpdateSticker();
        }

        private void UpdateStats()
        {
            var total = _db.Activities.Where(a => a.ClientId == _currentClient!.Id).Sum(a => (int?)a.Minutes) ?? 0;
            var count = _db.Activities.Count(a => a.ClientId == _currentClient!.Id);
            lblTotal.Text = $"Всего тренировок: {count} | Общее время: {total / 60}ч {total % 60}мин";
            UpdateSticker();
        }

        private void UpdateSticker()
        {
            var totalToday = _db.Activities.Where(a => a.ClientId == _currentClient!.Id && a.Date.Date == dtpDate.Value.Date).Sum(a => (int?)a.Minutes) ?? 0;
            stickerPanel.BackColor = totalToday < 30 ? Color.Gold : (totalToday <= 90 ? Color.LimeGreen : Color.Red);
            stickerPanel.Controls.Clear();
            Label lbl = new Label()
            {
                Text = $"{totalToday} мин",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            stickerPanel.Controls.Add(lbl);
        }
    }
}
