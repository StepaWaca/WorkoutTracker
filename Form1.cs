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
        private Workout? _currentWorkout;

        // Контролы
        private ComboBox cmbClient;
        private ListBox lstPrograms;
        private ListBox lstExercises;
        private DataGridView dgvWorkouts;
        private DataGridView dgvWorkoutExercises;
        private DateTimePicker dtpDate;
        private Panel stickerPanel;
        private Label lblTotal;
        private TextBox txtProgramName, txtProgramType;
        private CheckBox chkProgramActive;
        private Label lblExercisesCount, lblWorkoutsCount;
        private ComboBox cmbExerciseForWorkout;
        private NumericUpDown numSets, numReps, numWeight, numDuration;
        private TextBox txtExerciseNotes;
        private Button btnAddToWorkout;
        private ListBox lstSelectedExercises;
        private TextBox txtWorkoutNotes;
        private Button btnSaveWorkout, btnCancelWorkout;

        public Form1()
        {
            _db = new WorkoutContext();
            _db.Database.EnsureCreated();
            this.Text = "Учет тренировок";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateControls();
            LoadClients();
        }

        private void CreateControls()
        {
            dtpDate = new DateTimePicker()
            {
                Location = new Point(400, 14),
                Size = new Size(120, 250),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            this.Controls.Add(dtpDate);

            // ========== ВЕРХНЯЯ ПАНЕЛЬ ==========
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

            // ========== ЛЕВАЯ ПАНЕЛЬ - ПРОГРАММЫ ==========
            GroupBox gbPrograms = new GroupBox() { Text = "Программы тренировок", Location = new Point(12, 50), Size = new Size(300, 450) };
            lstPrograms = new ListBox() { Location = new Point(10, 25), Size = new Size(280, 150) };
            lstPrograms.DisplayMember = "Name";
            lstPrograms.SelectedIndexChanged += (s, e) => SelectProgram();

            Button btnAddProgram = new Button() { Location = new Point(10, 185), Size = new Size(90, 30), Text = "Добавить" };
            Button btnEditProgram = new Button() { Location = new Point(105, 185), Size = new Size(90, 30), Text = "Изменить" };
            Button btnDelProgram = new Button() { Location = new Point(200, 185), Size = new Size(90, 30), Text = "Удалить" };

            btnAddProgram.Click += (s, e) => AddEditProgram(null);
            btnEditProgram.Click += (s, e) => AddEditProgram(_selectedProgram);
            btnDelProgram.Click += (s, e) => DeleteProgram();

            // Детали программы
            GroupBox gbDetails = new GroupBox() { Text = "Детали программы", Location = new Point(10, 225), Size = new Size(280, 210) };
            txtProgramName = new TextBox() { Location = new Point(80, 25), Size = new Size(190, 25), ReadOnly = true };
            txtProgramType = new TextBox() { Location = new Point(80, 60), Size = new Size(190, 25), ReadOnly = true };
            chkProgramActive = new CheckBox()
            {
                Text = "Активна",
                Location = new Point(80, 95),
                Size = new Size(100, 25),
                Enabled = false
            };
            lblExercisesCount = new Label() { Text = "Упражнений: 0", Location = new Point(10, 130), Size = new Size(150, 25), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblWorkoutsCount = new Label() { Text = "Тренировок: 0", Location = new Point(10, 155), Size = new Size(150, 25), Font = new Font("Arial", 9, FontStyle.Bold) };

            gbDetails.Controls.AddRange(new Control[] {
                new Label() { Text = "Название:", Location = new Point(10, 30) , Size = new Size(65,16)},txtProgramName,
                new Label() { Text = "Тип:", Location = new Point(10, 65) , Size = new Size(60,16)}, txtProgramType, chkProgramActive,
                lblExercisesCount, lblWorkoutsCount
            });

            gbPrograms.Controls.AddRange(new Control[] { lstPrograms, btnAddProgram, btnEditProgram, btnDelProgram, gbDetails });

            // ========== СРЕДНЯЯ ПАНЕЛЬ - УПРАЖНЕНИЯ ПРОГРАММЫ ==========
            GroupBox gbExercises = new GroupBox() { Text = "Упражнения программы", Location = new Point(320, 50), Size = new Size(300, 450) };
            lstExercises = new ListBox() { Location = new Point(10, 25), Size = new Size(280, 300) };
            lstExercises.DisplayMember = "Name";

            Button btnAddExercise = new Button() { Location = new Point(10, 335), Size = new Size(90, 30), Text = "Добавить" };
            Button btnDelExercise = new Button() { Location = new Point(105, 335), Size = new Size(90, 30), Text = "Удалить" };
            Button btnToggleActive = new Button() { Location = new Point(200, 335), Size = new Size(90, 30), Text = "Актив" };

            btnAddExercise.Click += (s, e) => AddExerciseToProgram();
            btnDelExercise.Click += (s, e) => RemoveExerciseFromProgram();
            btnToggleActive.Click += (s, e) => ToggleExerciseActive();

            gbExercises.Controls.AddRange(new Control[] { lstExercises, btnAddExercise, btnDelExercise, btnToggleActive });

            // ========== ПРАВАЯ ПАНЕЛЬ - ТРЕНИРОВКИ ==========
            GroupBox gbWorkouts = new GroupBox() { Text = "Тренировки", Location = new Point(630, 50), Size = new Size(650, 250) };

            dgvWorkouts = new DataGridView()
            {
                Location = new Point(10, 25),
                Size = new Size(630, 180),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvWorkouts.SelectionChanged += (s, e) => SelectWorkout();

            Button btnNewWorkout = new Button() { Location = new Point(10, 215), Size = new Size(120, 30), Text = "Новая тренировка" };
            Button btnDeleteWorkout = new Button() { Location = new Point(140, 215), Size = new Size(100, 30), Text = "Удалить" };
            Button btnRefreshWorkouts = new Button() { Location = new Point(250, 215), Size = new Size(100, 30), Text = "Обновить" };

            btnNewWorkout.Click += (s, e) => StartNewWorkout();
            btnDeleteWorkout.Click += (s, e) => DeleteWorkout();
            btnRefreshWorkouts.Click += (s, e) => LoadWorkouts();

            gbWorkouts.Controls.AddRange(new Control[] { dgvWorkouts, btnNewWorkout, btnDeleteWorkout, btnRefreshWorkouts });

            // ========== ДЕТАЛИ ТРЕНИРОВКИ ==========
            GroupBox gbWorkoutDetails = new GroupBox() { Text = "Детали тренировки", Location = new Point(630, 310), Size = new Size(650, 190) };

            dgvWorkoutExercises = new DataGridView()
            {
                Location = new Point(10, 25),
                Size = new Size(500, 150),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Button btnEditWorkout = new Button() { Location = new Point(520, 25), Size = new Size(120, 30), Text = "Редактировать" };
            btnEditWorkout.Click += (s, e) => EditWorkout();

            gbWorkoutDetails.Controls.AddRange(new Control[] { dgvWorkoutExercises, btnEditWorkout });

            // ========== СТАТИСТИКА ==========
            GroupBox gbStats = new GroupBox() { Text = "Статистика", Location = new Point(12, 510), Size = new Size(1268, 100) };
            lblTotal = new Label() { Location = new Point(10, 25), Size = new Size(600, 25), Font = new Font("Arial", 10, FontStyle.Bold) };
            stickerPanel = new Panel() { Location = new Point(10, 55), Size = new Size(200, 35), BorderStyle = BorderStyle.FixedSingle };

            Button btnProgramReport = new Button() { Location = new Point(230, 55), Size = new Size(150, 35), Text = "Отчет по программе" };
            Button btnAllReport = new Button() { Location = new Point(390, 55), Size = new Size(150, 35), Text = "Общий отчет" };
            Button btnDailyReport = new Button() { Location = new Point(550, 55), Size = new Size(150, 35), Text = "Дневной отчет" };

            btnProgramReport.Click += (s, e) => ShowProgramReport();
            btnAllReport.Click += (s, e) => ShowAllReport();
            btnDailyReport.Click += (s, e) => ShowDailyReport();

            gbStats.Controls.AddRange(new Control[] { lblTotal, stickerPanel, btnProgramReport, btnAllReport, btnDailyReport });

            this.Controls.AddRange(new Control[] { lblClient, cmbClient, btnAddClient, gbPrograms, gbExercises, gbWorkouts, gbWorkoutDetails, gbStats });
        }

        private void SelectProgram()
        {
            _selectedProgram = lstPrograms.SelectedItem as WorkoutProgram;
            if (_selectedProgram != null)
            {
                txtProgramName.Text = _selectedProgram.Name;
                txtProgramType.Text = _selectedProgram.Type;
                chkProgramActive.Checked = _selectedProgram.IsActive;

                var exercises = _db.ProgramExercises
                    .Include(pe => pe.Exercise)
                    .Where(pe => pe.ProgramId == _selectedProgram.Id)
                    .Select(pe => pe.Exercise)
                    .ToList();
                lstExercises.DataSource = exercises;
                lstExercises.DisplayMember = "Name";
                lblExercisesCount.Text = $"Упражнений: {exercises.Count}";

                LoadWorkouts();
            }
        }

        private void AddEditProgram(WorkoutProgram? program)
        {
            Form dialog = new Form() { Size = new Size(400, 300), Text = program == null ? "Новая программа" : "Редактирование", StartPosition = FormStartPosition.CenterParent };

            TextBox txtName = new TextBox() { Location = new Point(120, 20), Size = new Size(230, 25) };
            TextBox txtType = new TextBox() { Location = new Point(120, 60), Size = new Size(230, 25) };
            TextBox txtDesc = new TextBox() { Location = new Point(120, 100), Size = new Size(230, 60), Multiline = true };
            CheckBox chkActive = new CheckBox() { Text = "Активна", Location = new Point(120, 175), Size = new Size(100, 25) };

            if (program != null)
            {
                txtName.Text = program.Name;
                txtType.Text = program.Type;
                txtDesc.Text = program.Description;
                chkActive.Checked = program.IsActive;
            }

            Button btnSave = new Button() { Text = "Сохранить", Location = new Point(120, 210), Size = new Size(100, 30) };
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Введите название"); return; }
                if (string.IsNullOrWhiteSpace(txtType.Text)) { MessageBox.Show("Введите тип"); return; }


                if (program == null)
                {
                    program = new WorkoutProgram();
                    _db.WorkoutPrograms.Add(program);
                }
                program.Name = txtName.Text;
                program.Type = txtType.Text;
                program.Description = txtDesc.Text;
                program.IsActive = chkActive.Checked;
                program.ClientId = _currentClient!.Id;
                _db.SaveChanges();
                dialog.Close();
                LoadData();
            };

            dialog.Controls.AddRange(new Control[] {
                new Label() { Text = "Название:", Location = new Point(20, 25), Size = new Size(80, 25) }, txtName,
                new Label() { Text = "Тип:", Location = new Point(20, 65), Size = new Size(80, 25) }, txtType,
                new Label() { Text = "Описание:", Location = new Point(20, 105), Size = new Size(80, 25) }, txtDesc,
                chkActive, btnSave
            });
            dialog.ShowDialog();
        }

        private void DeleteProgram()
        {
            if (_selectedProgram == null) { MessageBox.Show("Выберите программу"); return; }

            if (MessageBox.Show($"Удалить программу '{_selectedProgram.Name}'?\nВсе упражнения и тренировки программы будут удалены!",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.WorkoutPrograms.Remove(_selectedProgram);
                _db.SaveChanges();
                LoadData();
            }
        }

        private void AddExerciseToProgram()
        {
            if (_selectedProgram == null) { MessageBox.Show("Выберите программу"); return; }

            string name = Microsoft.VisualBasic.Interaction.InputBox("Название упражнения:", "Добавить упражнение", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Сначала добавляем в справочник
                var exercise = new Exercise { Name = name, ClientId = _currentClient!.Id, IsActive = true };
                _db.Exercises.Add(exercise);
                _db.SaveChanges();

                // Затем связываем с программой
                var programExercise = new ProgramExercise
                {
                    ProgramId = _selectedProgram.Id,
                    ExerciseId = exercise.Id,
                    TargetSets = 3,
                    TargetReps = 10
                };
                _db.ProgramExercises.Add(programExercise);
                _db.SaveChanges();

                LoadData();
            }
        }

        private void RemoveExerciseFromProgram()
        {
            if (_selectedProgram == null) { MessageBox.Show("Выберите программу"); return; }
            if (lstExercises.SelectedItem is Exercise ex)
            {
                var programExercise = _db.ProgramExercises.FirstOrDefault(pe => pe.ProgramId == _selectedProgram.Id && pe.ExerciseId == ex.Id);
                if (programExercise != null)
                {
                    _db.ProgramExercises.Remove(programExercise);
                    _db.SaveChanges();
                    LoadData();
                }
            }
        }

        private void ToggleExerciseActive()
        {
            if (lstExercises.SelectedItem is Exercise ex)
            {
                ex.IsActive = !ex.IsActive;
                _db.SaveChanges();
                LoadData();
            }
        }

        private void StartNewWorkout()
        {
            if (_selectedProgram == null)
            {
                MessageBox.Show("Выберите программу");
                return;
            }

            Form workoutForm = new Form()
            {
                Size = new Size(700, 600),
                Text = $"Новая тренировка - {_selectedProgram.Name}",
                StartPosition = FormStartPosition.CenterParent
            };

            // Выбор даты
            DateTimePicker dtpWorkoutDate = new DateTimePicker()
            {
                Location = new Point(120, 20),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            // Список доступных упражнений
            Label lblAvailable = new Label() { Text = "Упражнения программы:", Location = new Point(20, 60), Size = new Size(150, 25), Font = new Font("Arial", 9, FontStyle.Bold) };
            ListBox lstAvailable = new ListBox() { Location = new Point(20, 90), Size = new Size(200, 250) };

            var programExercises = _db.ProgramExercises
                .Include(pe => pe.Exercise)
                .Where(pe => pe.ProgramId == _selectedProgram.Id && pe.Exercise!.IsActive)
                .Select(pe => pe.Exercise)
                .ToList();
            lstAvailable.DataSource = programExercises;
            lstAvailable.DisplayMember = "Name";

            // Список выбранных упражнений
            Label lblSelected = new Label() { Text = "Выбранные упражнения:", Location = new Point(240, 60), Size = new Size(150, 25), Font = new Font("Arial", 9, FontStyle.Bold) };
            ListBox lstSelected = new ListBox() { Location = new Point(240, 90), Size = new Size(200, 250) };
            lstSelected.DisplayMember = "Name";

            // Панель для ввода параметров
            GroupBox gbParams = new GroupBox() { Text = "Параметры упражнения", Location = new Point(460, 90), Size = new Size(210, 250) };

            NumericUpDown numSets = new NumericUpDown() { Minimum = 1, Maximum = 20, Value = 3, Location = new Point(120, 30), Size = new Size(80, 25) };
            NumericUpDown numReps = new NumericUpDown() { Minimum = 1, Maximum = 100, Value = 10, Location = new Point(120, 65), Size = new Size(80, 25) };
            NumericUpDown numWeight = new NumericUpDown() { Minimum = 0, Maximum = 500, Value = 50, Location = new Point(120, 100), Size = new Size(80, 25) };
            NumericUpDown numDuration = new NumericUpDown() { Minimum = 1, Maximum = 180, Value = 10, Location = new Point(120, 135), Size = new Size(80, 25) };
            TextBox txtNotes = new TextBox() { Location = new Point(20, 190), Size = new Size(180, 55), Multiline = true };

            gbParams.Controls.AddRange(new Control[] {
                new Label() { Text = "Подходы:", Location = new Point(20, 35), Size = new Size(60, 25) }, numSets,
                new Label() { Text = "Повторения:", Location = new Point(20, 70), Size = new Size(100, 25) }, numReps,
                new Label() { Text = "Вес (кг):", Location = new Point(20, 105), Size = new Size(60, 25) }, numWeight,
                new Label() { Text = "Время (мин):", Location = new Point(20, 140), Size = new Size(100, 16) }, numDuration,
                new Label() { Text = "Заметки:", Location = new Point(20, 175), Size = new Size(60, 16) }, txtNotes
            });

            Button btnAdd = new Button() { Text = "Добавить »", Location = new Point(20, 355), Size = new Size(100, 30) };
            Button btnRemove = new Button() { Text = "« Удалить", Location = new Point(130, 355), Size = new Size(100, 30) };

            // Список выбранных упражнений (храним как отдельные объекты)
            List<WorkoutExercise> selectedExercises = new List<WorkoutExercise>();

            // При добавлении создаем НОВЫЙ объект
            btnAdd.Click += (s, e) =>
            {
                if (lstAvailable.SelectedItem is Exercise ex)
                {
                    // Создаем новый объект для каждого добавленного упражнения
                    var we = new WorkoutExercise
                    {
                        ExerciseId = ex.Id,
                        Exercise = ex,
                        Sets = (int)numSets.Value,
                        Reps = (int)numReps.Value,
                        Weight = (int)numWeight.Value,
                        Duration = (int)numDuration.Value,
                        Notes = txtNotes.Text
                    };
                    selectedExercises.Add(we);

                    // Обновляем список через обычный ListBox без DataBinding
                    lstSelected.Items.Clear();
                    foreach (var item in selectedExercises)
                    {
                        lstSelected.Items.Add($"{item.Exercise?.Name} - {item.Sets}x{item.Reps} (вес {item.Weight}кг) {item.Duration}мин");
                    }
                }
            };

            // При удалении
            btnRemove.Click += (s, e) =>
            {
                int index = lstSelected.SelectedIndex;
                if (index >= 0)
                {
                    selectedExercises.RemoveAt(index);
                    lstSelected.Items.Clear();
                    foreach (var item in selectedExercises)
                    {
                        lstSelected.Items.Add($"{item.Exercise?.Name} - {item.Sets}x{item.Reps} (вес {item.Weight}кг) {item.Duration}мин");
                    }
                }
            };

            TextBox txtWorkoutNotes = new TextBox() { Location = new Point(120, 400), Size = new Size(350, 60), Multiline = true };

            Button btnSaveWorkout = new Button() { Text = "Сохранить тренировку", Location = new Point(200, 480), Size = new Size(150, 40), BackColor = Color.LightGreen };
            btnSaveWorkout.Click += (s, e) =>
            {
                if (selectedExercises.Count == 0) { MessageBox.Show("Добавьте хотя бы одно упражнение"); return; }

                DateTime date = dtpDate.Value.Date; 

                int totalDuration = selectedExercises.Sum(ex => ex.Duration);

                if (!CheckDailyLimit(dtpWorkoutDate.Value,totalDuration))
                {
                    return;
                }

                var workout = new Workout
                {
                    Date = dtpWorkoutDate.Value,
                    TotalDuration = totalDuration,
                    Notes = txtWorkoutNotes.Text,
                    ClientId = _currentClient!.Id,
                    ProgramId = _selectedProgram.Id
                };
                _db.Workouts.Add(workout);
                _db.SaveChanges();

                foreach (var we in selectedExercises)
                {
                    we.WorkoutId = workout.Id;
                    _db.WorkoutExercises.Add(we);
                }
                _db.SaveChanges();

                workoutForm.Close();
                LoadWorkouts();
                UpdateStats();
                MessageBox.Show($"Тренировка сохранена!\nВремя: {totalDuration} минут", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            workoutForm.Controls.AddRange(new Control[] {
                new Label() { Text = "Дата:", Location = new Point(20, 25), Size = new Size(50, 25) }, dtpWorkoutDate,
                lblAvailable, lstAvailable, lblSelected, lstSelected, gbParams, btnAdd, btnRemove,
                new Label() { Text = "Общие заметки:", Location = new Point(20, 410), Size = new Size(90, 25) }, txtWorkoutNotes, btnSaveWorkout
            });

            workoutForm.ShowDialog();
        }

        private void EditWorkout()
        {
            if (_currentWorkout == null) { MessageBox.Show("Выберите тренировку"); return; }

            // Загружаем тренировку заново с данными
            _currentWorkout = _db.Workouts
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .FirstOrDefault(w => w.Id == _currentWorkout.Id);

            if (_currentWorkout == null) return;

            Form editForm = new Form()
            {
                Size = new Size(750, 650),
                Text = $"Редактирование тренировки от {_currentWorkout.Date:dd.MM.yyyy}",
                StartPosition = FormStartPosition.CenterParent
            };

            // Дата
            DateTimePicker dtpDate = new DateTimePicker()
            {
                Location = new Point(120, 20),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short,
                Value = _currentWorkout.Date
            };

            // Список выбранных упражнений
            ListBox lstSelected = new ListBox()
            {
                Location = new Point(250, 60),
                Size = new Size(200, 300),
                DisplayMember = "DisplayText"
            };

            // Создаем список для отображения
            var selectedList = _currentWorkout.WorkoutExercises.Select(we => new
            {
                we.Id,
                DisplayText = $"{we.Exercise?.Name} - {we.Sets}x{we.Reps} (вес {we.Weight}кг) {we.Duration}мин",
                we.Sets,
                we.Reps,
                we.Weight,
                we.Duration,
                we.Notes
            }).ToList();

            lstSelected.DataSource = selectedList;
            lstSelected.DisplayMember = "DisplayText";

            // Панель редактирования параметров
            GroupBox gbParams = new GroupBox() { Text = "Параметры упражнения", Location = new Point(470, 60), Size = new Size(240, 250) };

            NumericUpDown numSets = new NumericUpDown() { Minimum = 1, Maximum = 20, Value = 3, Location = new Point(120, 30), Size = new Size(100, 25) };
            NumericUpDown numReps = new NumericUpDown() { Minimum = 1, Maximum = 100, Value = 10, Location = new Point(120, 65), Size = new Size(100, 25) };
            NumericUpDown numWeight = new NumericUpDown() { Minimum = 0, Maximum = 500, Value = 50, Location = new Point(120, 100), Size = new Size(100, 25) };
            NumericUpDown numDuration = new NumericUpDown() { Minimum = 1, Maximum = 180, Value = 10, Location = new Point(120, 135), Size = new Size(100, 25) };
            TextBox txtExNotes = new TextBox() { Location = new Point(20, 190), Size = new Size(200, 55), Multiline = true };

            gbParams.Controls.AddRange(new Control[] {
        new Label() { Text = "Подходы:", Location = new Point(20, 35), Size = new Size(60, 25) }, numSets,
        new Label() { Text = "Повторения:", Location = new Point(20, 70), Size = new Size(100, 25) }, numReps,
        new Label() { Text = "Вес (кг):", Location = new Point(20, 105), Size = new Size(70, 25) }, numWeight,
        new Label() { Text = "Время (мин):", Location = new Point(20, 140), Size = new Size(80, 25) }, numDuration,
        new Label() { Text = "Заметки:", Location = new Point(20, 175), Size = new Size(60, 16) }, txtExNotes
    });

            Button btnUpdate = new Button() { Text = "Обновить", Location = new Point(20, 245), Size = new Size(100, 30) };
            Button btnRemove = new Button() { Text = "Удалить", Location = new Point(130, 245), Size = new Size(100, 30) };

            // Загружаем параметры выбранного упражнения
            lstSelected.SelectedIndexChanged += (s, e) =>
            {
                if (lstSelected.SelectedItem != null)
                {
                    dynamic selected = lstSelected.SelectedItem;
                    var we = _currentWorkout.WorkoutExercises.FirstOrDefault(x => x.Id == selected.Id);
                    if (we != null)
                    {
                        numSets.Value = we.Sets;
                        numReps.Value = we.Reps;
                        numWeight.Value = we.Weight;
                        numDuration.Value = we.Duration;
                        txtExNotes.Text = we.Notes;
                    }
                }
            };

            // Обновление параметров
            btnUpdate.Click += (s, e) =>
            {
                if (lstSelected.SelectedItem != null)
                {


                    dynamic selected = lstSelected.SelectedItem;
                    var we = _currentWorkout.WorkoutExercises.FirstOrDefault(x => x.Id == selected.Id);
                    if (we != null)
                    {
                        we.Sets = (int)numSets.Value;
                        we.Reps = (int)numReps.Value;
                        we.Weight = (int)numWeight.Value;
                        we.Duration = (int)numDuration.Value;
                        we.Notes = txtExNotes.Text;

                        // Обновляем отображение
                        var newList = _currentWorkout.WorkoutExercises.Select(w => new
                        {
                            w.Id,
                            DisplayText = $"{w.Exercise?.Name} - {w.Sets}x{w.Reps} (вес {w.Weight}кг) {w.Duration}мин",
                            w.Sets,
                            w.Reps,
                            w.Weight,
                            w.Duration,
                            w.Notes
                        }).ToList();
                        lstSelected.DataSource = null;
                        lstSelected.DataSource = newList;
                        lstSelected.DisplayMember = "DisplayText";
                    }
                }
            };

            // Удаление упражнения
            btnRemove.Click += (s, e) =>
            {
                if (lstSelected.SelectedItem != null)
                {
                    dynamic selected = lstSelected.SelectedItem;
                    var we = _currentWorkout.WorkoutExercises.FirstOrDefault(x => x.Id == selected.Id);
                    if (we != null)
                    {
                        _currentWorkout.WorkoutExercises.Remove(we);
                        var newList = _currentWorkout.WorkoutExercises.Select(w => new
                        {
                            w.Id,
                            DisplayText = $"{w.Exercise?.Name} - {w.Sets}x{w.Reps} (вес {w.Weight}кг) {w.Duration}мин",
                            w.Sets,
                            w.Reps,
                            w.Weight,
                            w.Duration,
                            w.Notes
                        }).ToList();
                        lstSelected.DataSource = null;
                        lstSelected.DataSource = newList;
                        lstSelected.DisplayMember = "DisplayText";
                    }
                }
            };

            // Заметки тренировки
            TextBox txtWorkoutNotes = new TextBox()
            {
                Location = new Point(120, 380),
                Size = new Size(350, 60),
                Multiline = true,
                Text = _currentWorkout.Notes
            };

            // Кнопка сохранения
            Button btnSave = new Button()
            {
                Text = "Сохранить изменения",
                Location = new Point(250, 460),
                Size = new Size(180, 40),
                BackColor = Color.LightGreen
            };

            btnSave.Click += (s, e) =>
            {
                _currentWorkout.Date = dtpDate.Value;
                _currentWorkout.Notes = txtWorkoutNotes.Text;
                _currentWorkout.TotalDuration = _currentWorkout.WorkoutExercises.Sum(we => we.Duration);

                if (!CheckDailyLimit(dtpDate.Value, _currentWorkout.TotalDuration, _currentWorkout.Id))
                {
                    return;
                }

                _db.SaveChanges();

                editForm.Close();
                LoadWorkouts();
                UpdateStats();
                MessageBox.Show("Тренировка обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            editForm.Controls.AddRange(new Control[] {
        new Label() { Text = "Дата:", Location = new Point(20, 25), Size = new Size(50, 25) }, dtpDate,
        new Label() { Text = "Выбранные упражнения:", Location = new Point(250, 35), Size = new Size(150, 25), Font = new Font("Arial", 9, FontStyle.Bold) },
        lstSelected, gbParams, btnUpdate, btnRemove,
        new Label() { Text = "Заметки тренировки:", Location = new Point(20, 390), Size = new Size(100, 25) }, txtWorkoutNotes,
        btnSave
    });

            editForm.ShowDialog();
        }

        private void SelectWorkout()
        {
            if (dgvWorkouts.SelectedRows.Count > 0)
            {
                int id = (int)dgvWorkouts.SelectedRows[0].Cells["Id"].Value;
                _currentWorkout = _db.Workouts
                    .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                    .FirstOrDefault(w => w.Id == id);

                if (_currentWorkout != null)
                {
                    var exercises = _currentWorkout.WorkoutExercises.Select(we => new
                    {
                        Упражнение = we.Exercise?.Name ?? "",
                        Подходы = we.Sets,
                        Повторения = we.Reps,
                        Вес = we.Weight,
                        Время = we.Duration,
                        Заметки = we.Notes ?? ""
                    }).ToList();
                    dgvWorkoutExercises.DataSource = null;
                    dgvWorkoutExercises.DataSource = exercises;
                }
            }
        }

        private void DeleteWorkout()
        {
            if (_currentWorkout == null)
            {
                MessageBox.Show("Выберите тренировку");
                return;
            }

            if (MessageBox.Show("Удалить тренировку?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // Сначала удаляем связанные WorkoutExercise
                var exercisesToDelete = _db.WorkoutExercises.Where(we => we.WorkoutId == _currentWorkout.Id);
                _db.WorkoutExercises.RemoveRange(exercisesToDelete);

                // Затем удаляем саму тренировку
                _db.Workouts.Remove(_currentWorkout);

                _db.SaveChanges();

                _currentWorkout = null;
                LoadWorkouts();
                UpdateStats();

                MessageBox.Show("Тренировка удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
            if (cmbClient.Items.Count > 0)
                cmbClient.SelectedIndex = 0;
            LoadData();
        }

        private void LoadData()
        {
            if (cmbClient.SelectedItem == null) return;
            _currentClient = (Client)cmbClient.SelectedItem;

            var programs = _db.WorkoutPrograms.Where(p => p.ClientId == _currentClient.Id).ToList();
            lstPrograms.DataSource = programs;

            if (programs.Any() && lstPrograms.SelectedItem == null)
                lstPrograms.SelectedIndex = 0;
        }

        private void LoadWorkouts()
        {
            if (_selectedProgram == null) return;

            var workouts = _db.Workouts
                .Where(w => w.ProgramId == _selectedProgram.Id)
                .OrderByDescending(w => w.Date)
                .Select(w => new { w.Id, Дата = w.Date.ToString("dd.MM.yyyy"), Длительность = w.TotalDuration, Заметки = w.Notes })
                .ToList();
            dgvWorkouts.DataSource = workouts;
            lblWorkoutsCount.Text = $"Тренировок: {workouts.Count}";
        }

        private void UpdateStats()
        {
            if (_currentClient == null) return;
            var total = _db.Workouts.Where(w => w.ClientId == _currentClient.Id).Sum(w => w.TotalDuration);
            var count = _db.Workouts.Count(w => w.ClientId == _currentClient.Id);
            lblTotal.Text = $"Всего тренировок: {count} | Общее время: {total / 60}ч {total % 60}мин | {total} минут";

            var todayTotal = _db.Workouts.Where(w => w.ClientId == _currentClient.Id && w.Date.Date == DateTime.Today).Sum(w => w.TotalDuration);
            stickerPanel.BackColor = todayTotal < 30 ? Color.Gold : (todayTotal <= 90 ? Color.LimeGreen : Color.Red);
            stickerPanel.Controls.Clear();
            stickerPanel.Controls.Add(new Label()
            {
                Text = $"Сегодня: {todayTotal} мин",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            });
        }

        private void ShowProgramReport()
        {
            if (_selectedProgram == null) { MessageBox.Show("Выберите программу"); return; }

            var workouts = _db.Workouts
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .Where(w => w.ProgramId == _selectedProgram.Id)
                .ToList();

            string report = $"ОТЧЕТ ПО ПРОГРАММЕ: {_selectedProgram.Name}\n";
            report += $"Тип: {_selectedProgram.Type}\n";
            report += new string('=', 39) + "\n\n";
            report += $"Всего тренировок: {workouts.Count}\n";
            report += $"Общее время: {workouts.Sum(w => w.TotalDuration)} минут\n\n";

            foreach (var w in workouts.OrderByDescending(w => w.Date))
            {
                report += $"\n{w.Date:dd.MM.yyyy} - {w.TotalDuration} мин\n";
                foreach (var we in w.WorkoutExercises)
                {
                    report += $"  • {we.Exercise?.Name}: {we.Sets}x{we.Reps} (вес {we.Weight}кг) - {we.Duration} мин\n";
                }
                if (!string.IsNullOrEmpty(w.Notes))
                    report += $"  Заметки: {w.Notes}\n";
            }

            MessageBox.Show(report, "Отчет по программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAllReport()
        {
            var allWorkouts = _db.Workouts
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .Where(w => w.ClientId == _currentClient!.Id)
                .OrderByDescending(w => w.Date)
                .ToList();

            string report = $"ОБЩИЙ ОТЧЕТ ДЛЯ: {_currentClient?.FullName}\n";
            report += new string('=', 39) + "\n\n";
            report += $"Всего тренировок: {allWorkouts.Count}\n";
            report += $"Общее время: {allWorkouts.Sum(w => w.TotalDuration)} минут\n\n";

            foreach (var w in allWorkouts)
            {
                report += $"\n{w.Date:dd.MM.yyyy} - {w.TotalDuration} мин";
                if (w.Program != null)
                    report += $" (программа: {w.Program.Name})";
                report += "\n";
                foreach (var we in w.WorkoutExercises)
                {
                    report += $"  • {we.Exercise?.Name}: {we.Sets}x{we.Reps} (вес {we.Weight}кг) - {we.Duration} мин\n";
                }
            }

            MessageBox.Show(report, "Общий отчет", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowDailyReport()
        {
            DateTime date = dtpDate.Value.Date;
            var workouts = _db.Workouts
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .Where(w => w.ClientId == _currentClient!.Id && w.Date.Date == date)
                .ToList();

            string report = $"ОТЧЕТ ЗА {date:dd.MM.yyyy}\n";
            report += new string('=', 39) + "\n\n";

            if (workouts.Any())
            {
                report += $"Всего тренировок: {workouts.Count}\n";
                report += $"Общее время: {workouts.Sum(w => w.TotalDuration)} минут\n\n";

                foreach (var w in workouts)
                {
                    report += $"Тренировка {w.Date:HH:mm}:\n";
                    foreach (var we in w.WorkoutExercises)
                    {
                        report += $"  • {we.Exercise?.Name}: {we.Sets}x{we.Reps} (вес {we.Weight}кг) - {we.Duration} мин\n";
                    }
                }
            }
            else
            {
                report += "Нет тренировок в этот день";
            }

            MessageBox.Show(report, "Дневной отчет", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private bool CheckDailyLimit(DateTime date, int additionalMinutes, int? excludeWorkoutId = null)
        {
            var workouts = _db.Workouts
                .Where(w => w.ClientId == _currentClient!.Id && w.Date.Date == date.Date);

            if (excludeWorkoutId.HasValue)
            {
                workouts = workouts.Where(w => w.Id != excludeWorkoutId.Value);
            }

            int totalMinutes = workouts.Sum(w => w.TotalDuration) + additionalMinutes;

            if (totalMinutes > 1440)
            {
                MessageBox.Show($"Превышение лимита! За день уже {totalMinutes - additionalMinutes} минут. Лимит 1440 минут.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }

}