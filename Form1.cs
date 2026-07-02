using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Group01
{
    public partial class Form1 : Form
    {
        string connString = @"Server=localhost;Database=StudentDB;Trusted_Connection=True;TrustServerCertificate=True;";
        bool isUpdating = false;
        private string selectedRegNum;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadStudentData();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtName.Text.Trim();
            string regNum = txtRegNum.Text.Trim();
            string programme = txtProgramme.Text.Trim();
            string gender = cmbGender.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(regNum) ||
                string.IsNullOrEmpty(programme) || string.IsNullOrEmpty(gender))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            string query = "";

            // Decide whether to Update or Insert based on our tracking flag
            if (isUpdating)
            {
                query = "UPDATE Students SET FullName = @FullName, Programme = @Programme, Gender = @Gender " +
                        "WHERE RegistrationNumber = @RegNum";
            }
            else
            {
                query = "INSERT INTO Students (FullName, RegistrationNumber, Programme, Gender) " +
                        "VALUES (@FullName, @RegNum, @Programme, @Gender)";
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@RegNum", regNum);
                    cmd.Parameters.AddWithValue("@Programme", programme);
                    cmd.Parameters.AddWithValue("@Gender", gender);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        if (isUpdating)
                            MessageBox.Show("Student updated successfully!");
                        else
                            MessageBox.Show("Student registered successfully!");

                        // Refresh Layout & Reset Form State
                        LoadStudentData();
                        ResetFormState();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database Error: " + ex.Message);
                    }
                }
            }
        }

        private void ResetFormState()
        {
                txtName.Clear();
                txtRegNum.Clear();
                txtProgramme.Clear();
                cmbGender.SelectedIndex = -1;

                // Crucial cleanups:
                txtRegNum.ReadOnly = false; // Allow typing numbers again for brand new students
                isUpdating = false;            // Reset flag back to default "Insert Mode"
                txtName.Focus();
            
        }

        private void dgvStudents_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudents.Rows[e.RowIndex];

                // 1. Populate textboxes with current data
                txtName.Text = row.Cells[0].Value?.ToString();
                txtRegNum.Text = row.Cells[1].Value?.ToString();
                txtProgramme.Text = row.Cells[2].Value?.ToString();
                cmbGender.SelectedItem = row.Cells[3].Value?.ToString();

                // 2. Lock the Registration Number textbox! 
                // Since it's the Primary Key in SQL, you cannot change the ID itself while editing.
                txtRegNum.ReadOnly = true;

                // 3. Flip our flag to true so the Register button knows we are editing
                isUpdating = true;
            }
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtRegNum.Clear();
            txtProgramme.Clear();
            cmbGender.SelectedIndex = -1;
            txtName.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void LoadStudentData()
        {
            string query = "SELECT FullName, RegistrationNumber, Programme, Gender FROM Students";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        conn.Open();
                        adapter.Fill(dt);

                        dgvStudents.DataSource = dt;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database Error: " + ex.Message);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStudentData();

            // 1. Highlight the entire row when clicked instead of a single cell
            dgvStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 2. Turn off cell editing (prevents the cursor from appearing inside cells)
            dgvStudents.ReadOnly = true;

            // 3. Prevent users from clicking into a blank bottom row to manually add data
            dgvStudents.AllowUserToAddRows = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Check if the user actually selected a student from the grid first
            if (string.IsNullOrEmpty(selectedRegNum))
            {
                MessageBox.Show("Please click on a student in the grid first before hitting delete.");
                return;
            }

            // Confirmation pop-up so you don't accidentally delete someone
            DialogResult confirm = MessageBox.Show("Are you sure you want to delete student " + selectedRegNum + "?",
                                                   "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.No) return;

            string query = "DELETE FROM Students WHERE RegistrationNumber = @RegNum";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RegNum", selectedRegNum);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Student deleted successfully!");

                        // Refresh the layout
                        LoadStudentData();
                        btnClear_Click(sender, e); // Wipes the textboxes clean
                        selectedRegNum = "";       // Reset our tracker variable
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting data: " + ex.Message);
                    }
                }
            }
     }


        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudents.Rows[e.RowIndex];

                // Use column position numbers instead of string names to prevent crashes!
                txtName.Text = row.Cells[0].Value?.ToString();
                selectedRegNum = row.Cells[1].Value?.ToString(); // Saves to our global delete variable
                txtRegNum.Text = selectedRegNum;
                txtProgramme.Text = row.Cells[2].Value?.ToString();
                cmbGender.SelectedItem = row.Cells[3].Value?.ToString();
            }
        }
    }
}