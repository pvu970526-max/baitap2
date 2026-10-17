using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsAppbai2.Models;

namespace WinFormsAppbai2
{
    public partial class Form1 : Form
    {
        private BindingList<LoanRecord> allRecords = new BindingList<LoanRecord>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // configure DataGridView columns
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.Columns.Clear();
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã phiếu", DataPropertyName = "TicketId" });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã SV", DataPropertyName = "StudentId" });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ tên", DataPropertyName = "StudentName" });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thiết bị", DataPropertyName = "Device" });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số lượng", DataPropertyName = "Quantity" });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày mượn", DataPropertyName = "BorrowDate", DefaultCellStyle = { Format = "yyyy-MM-dd" } });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hạn trả", DataPropertyName = "DueDate", DefaultCellStyle = { Format = "yyyy-MM-dd" } });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày trả", DataPropertyName = "ReturnDateDisplay" });
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "Status" });

            bindingSource.DataSource = allRecords;
            dgvRecords.DataSource = bindingSource;

            cmbFilter.SelectedIndex = 0;
            dtpBorrow.Value = DateTime.Today;
            dtpDue.Value = DateTime.Today.AddDays(7);

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            toolStripStatusLabelTotal.Text = $"Tổng: {allRecords.Count}";
            var overdue = allRecords.Count(r => r.Status == "Quá hạn");
            toolStripStatusLabelOverdue.Text = $"Quá hạn: {overdue}";
        }

        private void RefreshView()
        {
            var text = txtSearch.Text?.Trim().ToLower() ?? string.Empty;
            var filter = cmbFilter.SelectedItem?.ToString() ?? "Tất cả";

            var items = allRecords.Where(r =>
                (string.IsNullOrEmpty(text) || r.StudentId.ToLower().Contains(text) || r.Device.ToLower().Contains(text))) ;

            if (filter == "Đang mượn") items = items.Where(r => r.Status == "Đang mượn");
            else if (filter == "Quá hạn") items = items.Where(r => r.Status == "Quá hạn");
            else if (filter == "Đã trả") items = items.Where(r => r.Status == "Đã trả");

            bindingSource.DataSource = new BindingList<LoanRecord>(items.ToList());
            dgvRecords.DataSource = bindingSource;
            RefreshStatus();
        }

        private bool ValidateInput(out LoanRecord temp)
        {
            errorProvider.Clear();
            temp = null;
            if (string.IsNullOrWhiteSpace(txtTicketId.Text))
            {
                errorProvider.SetError(txtTicketId, "Mã phiếu không được để trống");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                errorProvider.SetError(txtStudentId, "Mã sinh viên không được để trống");
                return false;
            }
            if (cmbDevice.SelectedItem == null || string.IsNullOrWhiteSpace(cmbDevice.Text))
            {
                errorProvider.SetError(cmbDevice, "Chọn thiết bị");
                return false;
            }
            if (numQuantity.Value <= 0)
            {
                errorProvider.SetError(numQuantity, "Số lượng phải > 0");
                return false;
            }
            if (dtpDue.Value.Date < dtpBorrow.Value.Date)
            {
                errorProvider.SetError(dtpDue, "Hạn trả không trước ngày mượn");
                return false;
            }
            if (dtpReturn.Checked && dtpReturn.Value.Date < dtpBorrow.Value.Date)
            {
                errorProvider.SetError(dtpReturn, "Ngày trả thực tế không trước ngày mượn");
                return false;
            }

            temp = new LoanRecord
            {
                TicketId = txtTicketId.Text.Trim(),
                StudentId = txtStudentId.Text.Trim(),
                StudentName = txtStudentName.Text.Trim(),
                Device = cmbDevice.Text,
                Quantity = (int)numQuantity.Value,
                BorrowDate = dtpBorrow.Value.Date,
                DueDate = dtpDue.Value.Date,
                ReturnDate = dtpReturn.Checked ? dtpReturn.Value.Date : (DateTime?)null
            };

            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out var rec)) return;
            if (allRecords.Any(r => r.TicketId == rec.TicketId))
            {
                MessageBox.Show("Mã phiếu đã tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            allRecords.Add(rec);
            RefreshView();
            ClearInputs();
        }

        private void ClearInputs()
        {
            txtTicketId.Clear();
            txtStudentId.Clear();
            txtStudentName.Clear();
            cmbDevice.SelectedIndex = -1;
            numQuantity.Value = 1;
            dtpBorrow.Value = DateTime.Today;
            dtpDue.Value = DateTime.Today.AddDays(7);
            dtpReturn.Checked = false;
        }

        private void dgvRecords_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRecords.CurrentRow == null) return;
            if (dgvRecords.CurrentRow.DataBoundItem is LoanRecord sel)
            {
                LoadToInputs(sel);
            }
        }

        private void LoadToInputs(LoanRecord sel)
        {
            txtTicketId.Text = sel.TicketId;
            txtStudentId.Text = sel.StudentId;
            txtStudentName.Text = sel.StudentName;
            cmbDevice.Text = sel.Device;
            numQuantity.Value = sel.Quantity;
            dtpBorrow.Value = sel.BorrowDate;
            dtpDue.Value = sel.DueDate;
            if (sel.ReturnDate.HasValue)
            {
                dtpReturn.Value = sel.ReturnDate.Value;
                dtpReturn.Checked = true;
            }
            else dtpReturn.Checked = false;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvRecords.CurrentRow == null) return;
            if (!ValidateInput(out var rec)) return;
            var ticket = rec.TicketId;
            var existing = allRecords.FirstOrDefault(r => r.TicketId == ticket);
            if (existing == null)
            {
                MessageBox.Show("Không tìm thấy phiếu để cập nhật", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // update fields
            existing.StudentId = rec.StudentId;
            existing.StudentName = rec.StudentName;
            existing.Device = rec.Device;
            existing.Quantity = rec.Quantity;
            existing.BorrowDate = rec.BorrowDate;
            existing.DueDate = rec.DueDate;
            existing.ReturnDate = rec.ReturnDate;

            // refresh bindings
            RefreshView();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRecords.CurrentRow == null) return;
            if (!(dgvRecords.CurrentRow.DataBoundItem is LoanRecord sel)) return;
            var ok = MessageBox.Show($"Xóa phiếu {sel.TicketId}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;
            var existing = allRecords.FirstOrDefault(r => r.TicketId == sel.TicketId);
            if (existing != null) allRecords.Remove(existing);
            RefreshView();
        }

        private void btnMarkReturned_Click(object sender, EventArgs e)
        {
            if (dgvRecords.CurrentRow == null) return;
            if (!(dgvRecords.CurrentRow.DataBoundItem is LoanRecord sel)) return;
            var existing = allRecords.FirstOrDefault(r => r.TicketId == sel.TicketId);
            if (existing == null) return;
            existing.ReturnDate = DateTime.Today;
            RefreshView();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshView();
        }

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshView();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (bindingSource.DataSource is BindingList<LoanRecord> list && list.Count > 0)
            {
                using var sfd = new SaveFileDialog();
                sfd.Filter = "CSV|*.csv";
                sfd.FileName = "export.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var sw = new StreamWriter(sfd.FileName);
                        sw.WriteLine("TicketId,StudentId,StudentName,Device,Quantity,BorrowDate,DueDate,ReturnDate,Status");
                        foreach (var r in list)
                        {
                            sw.WriteLine($"{r.TicketId},{r.StudentId},{r.StudentName},{r.Device},{r.Quantity},{r.BorrowDate:yyyy-MM-dd},{r.DueDate:yyyy-MM-dd},{r.ReturnDateDisplay},{r.Status}");
                        }
                        MessageBox.Show("Export xong", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có dữ liệu để xuất", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

