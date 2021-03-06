﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace MasterContactListApplication
{
    public partial class ListForm : Form
    {
        DataSet ds = new DataSet();
        DataTable searchDt = new DataTable();
        string constring = System.Configuration.ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlDataAdapter da;
        int indexOfRowToDelete;

        Boolean searchDataGridHasChanged;
        Boolean listDataGridHasChanged;
        string searchQuery;
        MasterContactListEntities db = new MasterContactListEntities();
     
        public ListForm()
        {

            InitializeComponent();
            this.CenterToScreen();
            da = new SqlDataAdapter("Select * from Contact_List", constring);
            SqlCommandBuilder cmb = new SqlCommandBuilder(da);
     
            da.Fill(ds, "Contact_List");
            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "Contact_List";

            searchDataGridHasChanged = false;
            dataGridView_SearchResults.AllowUserToAddRows = false;
          

            //dataGridView_SearchResults.ScrollBars = ScrollBars.Horizontal;
            //dataGridView_SearchResults.AutoSize = true;
            
        }

        private void Combobox_LastOrEmail_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Combobox_LastOrEmail.SelectedIndex == 0)
            {
                Label_LastOrEmail.Text = Combobox_LastOrEmail.Text + ": ";
                Label_LastOrEmail.Visible = true;
                Textbox_LastOrEmail.Visible = true;
                Button_Search.Visible = true;
                
            }
            else if(Combobox_LastOrEmail.SelectedIndex == 1)
            {
                Label_LastOrEmail.Text = Combobox_LastOrEmail.Text + ": ";
                Label_LastOrEmail.Visible = true;
                Textbox_LastOrEmail.Visible = true;
                Button_Search.Visible = true;
            }
            else
            {
                MessageBox.Show("Whoops");
            }
        }

        private void Button_Search_Click(object sender, EventArgs e)
        {

            searchDt.Clear();

            if (Textbox_LastOrEmail.TextLength > 0)
            {
                Label_LastOrEmail.ForeColor = Color.Black;

                if (Combobox_LastOrEmail.SelectedIndex == 0)
                {
                    searchQuery = "SELECT * FROM Contact_List WHERE Last_Name = '" + Textbox_LastOrEmail.Text + "'";

                    using (SqlConnection con = new SqlConnection(constring))
                    {
                        da = new SqlDataAdapter(searchQuery, con);
                        da.Fill(searchDt);

                        if (searchDt.Rows.Count > 0)
                        {
                            dataGridView_SearchResults.DataSource = searchDt;
                            dataGridView_SearchResults.Visible = true;
              
                            searchDataGridHasChanged = false;

                            Label_SearchResults.Text = searchDt.Rows.Count.ToString() + " Result(s) found with Last Name: " + Textbox_LastOrEmail.Text;
                            Label_SearchResults.ForeColor = Color.Black;
                            Label_SearchResults.Visible = true;

                            Button_SaveSearchEdit.BackColor = Color.Transparent;
                            Button_SaveSearchEdit.Visible = true;

                        }
                        else
                        {
                            Label_SearchResults.Text = searchDt.Rows.Count.ToString() + " Result(s) found with Last Name: " + Textbox_LastOrEmail.Text;
                            Label_SearchResults.ForeColor = Color.Red;
                            Label_SearchResults.Visible = true;
                            dataGridView_SearchResults.Visible = false;
                            Button_SaveSearchEdit.Visible = false;

                        }
                    }
                }
                else if (Combobox_LastOrEmail.SelectedIndex == 1)
                {
                    searchQuery = "SELECT * FROM Contact_List WHERE email_address = '" + Textbox_LastOrEmail.Text + "'";

                    using (SqlConnection con = new SqlConnection(constring))
                    {
                        da = new SqlDataAdapter(searchQuery, con);
                        da.Fill(searchDt);

                        if (searchDt.Rows.Count > 0)
                        {
                            dataGridView_SearchResults.DataSource = searchDt;
                            dataGridView_SearchResults.Visible = true;

                            searchDataGridHasChanged = false;

                            Label_SearchResults.Text = searchDt.Rows.Count.ToString() + " Result(s) found with Email Address: " + Textbox_LastOrEmail.Text;
                            Label_SearchResults.ForeColor = Color.Black;
                            Label_SearchResults.Visible = true;

                            Button_SaveSearchEdit.BackColor = Color.Transparent;
                            Button_SaveSearchEdit.Visible = true;
                        }
                        else
                        {
                            Label_SearchResults.Text = searchDt.Rows.Count.ToString() + " Result(s) found with Email Address: " + Textbox_LastOrEmail.Text;
                            Label_SearchResults.ForeColor = Color.Red;
                            Label_SearchResults.Visible = true;
                            dataGridView_SearchResults.Visible = false;

                            Button_SaveSearchEdit.Visible = false;
                        }
                    }
                }
            }
            else
            {
                Label_LastOrEmail.ForeColor = Color.Red;
            }
            
        }

        private void Button_SaveSearchEdit_Click(object sender, EventArgs e)
        {
            if (searchDataGridHasChanged == true)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(constring))
                    {
                        SqlDataAdapter searchDa = new SqlDataAdapter(searchQuery, con);
                        SqlCommandBuilder cmd = new SqlCommandBuilder(searchDa);
                        var cm = dataGridView_SearchResults.BindingContext[searchDt];
                        cm.EndCurrentEdit();


                        con.Open();
                        searchDa.Update(searchDt);
                        con.Close();

                        searchDataGridHasChanged = false;

                        MessageBox.Show("Upates Saved Succesfully");
                        Button_SaveSearchEdit.BackColor = Color.Transparent;


                        da = new SqlDataAdapter("Select * From Contact_List", con);
                        SqlCommandBuilder cmd2 = new SqlCommandBuilder(da);

                        ds.Tables[0].Clear();
                        da.Fill(ds.Tables[0]);
                        dataGridView1.DataSource = ds.Tables[0];

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No changes to save. Operation Aborted.");
            }
        }

       

        private void dataGridView_SearchResults_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            searchDataGridHasChanged = true;
            Button_SaveSearchEdit.BackColor = Color.Red;
        }

        private void ListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridView_SearchResults.EndEdit();
            dataGridView1.EndEdit();

            if(searchDataGridHasChanged == true || listDataGridHasChanged == true)
            {
                SaveForm saveForm = new SaveForm();
                saveForm.ShowDialog();

                if(saveForm.WantToSave == true)
                {
                    if (Tab_Control.SelectedTab == Tab_Search)
                    {
                        try
                        {
                            using (SqlConnection con = new SqlConnection(constring))
                            {
                                SqlDataAdapter searchDa = new SqlDataAdapter(searchQuery, con);
                                SqlCommandBuilder cmd = new SqlCommandBuilder(searchDa);
                                var cm = dataGridView_SearchResults.BindingContext[searchDt];
                                cm.EndCurrentEdit();


                                con.Open();
                                searchDa.Update(searchDt);
                                con.Close();

                                searchDataGridHasChanged = false;

                                MessageBox.Show("Upates Saved Succesfully");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        try
                        {
                            using (SqlConnection con = new SqlConnection(constring))
                            {
                                SqlDataAdapter listDa = new SqlDataAdapter("SELECT * FROM Contact_List", con);
                                SqlCommandBuilder cmd = new SqlCommandBuilder(listDa);
                                var cm = dataGridView1.BindingContext[ds.Tables[0]];
                                cm.EndCurrentEdit();


                                con.Open();
                                listDa.Update(ds.Tables[0]);
                                con.Close();

                                listDataGridHasChanged = false;

                                MessageBox.Show("Upates Saved Succesfully");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            e.Cancel = true;
                        }
                    }
                }
            }

            


        }

        private void Button_AddContact_Click(object sender, EventArgs e)
        {
            if(Textbox_FName.Text.Length > 0 && Textbox_Lname.Text.Length > 0 && Textbox_Email.Text.Length > 0)
            {
                Contact_List newContact = new Contact_List();

                newContact.First_Name = Textbox_FName.Text;
                newContact.Last_Name = Textbox_Lname.Text;
                newContact.Email_Address = Textbox_Email.Text;
                newContact.Phone_Number = Textbox_PhoneNumber.Text;
                newContact.Title = Textbox_Title.Text;
                newContact.Contact_Type = Textbox_ContactType.Text;
                newContact.Department = Textbox_Department.Text;
                newContact.Region = (int)numericUpDown1_Region.Value;
                newContact.VISN = (int)numericUpDown2_VISN.Value;
                newContact.Company = Textbox_Company.Text;
                newContact.Contract_Coding_Compnay = Textbox_ContractCodingCompany.Text;
                newContact.City = Textbox_City.Text;
                newContact.State = Textbox_State.Text;
                newContact.AE = Textbox_AE.Text;
                newContact.E_Bulletin = Textbox_EBulletin.Text;

                if(checkedListBox1.GetItemChecked(0) == true)
                {
                    newContact.Decision_Maker = true;
                }
                else
                {
                    newContact.Decision_Maker = false;
                }

                if (checkedListBox1.GetItemChecked(1) == true)
                {
                    newContact.Budget_Holder = true;
                }
                else
                {
                    newContact.Budget_Holder = false;
                }

                if (checkedListBox1.GetItemChecked(2) == true)
                {
                    newContact.EPS = true;
                }
                else
                {
                    newContact.EPS = false;
                }

                if (checkedListBox1.GetItemChecked(3) == true)
                {
                    newContact.Simulator = true;
                }
                else
                {
                    newContact.Simulator = false;
                }

                if (checkedListBox1.GetItemChecked(4) == true)
                {
                    newContact.VACC = true;
                }
                else
                {
                    newContact.VACC = false;
                }

                if (checkedListBox1.GetItemChecked(5) == true)
                {
                    newContact.VACC_R_Notes = true;
                }
                else
                {
                    newContact.VACC_R_Notes = false;
                }

                if (checkedListBox1.GetItemChecked(6) == true)
                {
                    newContact.CDI_Pro = true;
                }
                else
                {
                    newContact.CDI_Pro = false;
                }

                if (checkedListBox1.GetItemChecked(7) == true)
                {
                    newContact.CDI_Pro_R_Notes = true;
                }
                else
                {
                    newContact.CDI_Pro_R_Notes = false;
                }

                if (checkedListBox1.GetItemChecked(8) == true)
                {
                    newContact.VERA_Az = true;
                }
                else
                {
                    newContact.VERA_Az = false;
                }

                if (checkedListBox1.GetItemChecked(9) == true)
                {
                    newContact.VERA_Az_R_Notes = true;
                }
                else
                {
                    newContact.VERA_Az_R_Notes = false;
                }

                if (checkedListBox1.GetItemChecked(10) == true)
                {
                    newContact.OI_T= true;
                }
                else
                {
                    newContact.OI_T = false;
                }

                if (checkedListBox1.GetItemChecked(11) == true)
                {
                    newContact.NHAC = true;
                }
                else
                {
                    newContact.NHAC = false;
                }

                if (checkedListBox1.GetItemChecked(12) == true)
                {
                    newContact.Svc_Interrupt_Restore = true;
                }
                else
                {
                    newContact.Svc_Interrupt_Restore = false;
                }

                if (checkedListBox1.GetItemChecked(13) == true)
                {
                    newContact.Webcast_Bulletin = true;
                }
                else
                {
                    newContact.Webcast_Bulletin = false;
                }

                if (checkedListBox1.GetItemChecked(14) == true)
                {
                    newContact.iSupport_Account = true;
                }
                else
                {
                    newContact.iSupport_Account = false;
                }

                if (checkedListBox1.GetItemChecked(15) == true)
                {
                    newContact.Bulletin_Reader = true;
                }
                else
                {
                    newContact.Bulletin_Reader = false;
                }

                if (checkedListBox1.GetItemChecked(16) == true)
                {
                    newContact.NPS_Fall_2016 = true;
                }
                else
                {
                    newContact.NPS_Fall_2016 = false;
                }

                if (checkedListBox1.GetItemChecked(17) == true)
                {
                    newContact.NPS_Spring_2016= true;
                }
                else
                {
                    newContact.NPS_Spring_2016 = false;
                }

                if (checkedListBox1.GetItemChecked(18) == true)
                {
                    newContact.NPS_Spring_2015 = true;
                }
                else
                {
                    newContact.NPS_Spring_2015 = false;
                }

                if (checkedListBox1.GetItemChecked(19) == true)
                {
                    newContact.OI_T_Building_Relationships = true;
                }
                else
                {
                    newContact.OI_T_Building_Relationships = false;
                }

                if (checkedListBox1.GetItemChecked(20) == true)
                {
                    newContact.Klas = true;
                }
                else
                {
                    newContact.Klas = false;
                }

                if (db.Contact_List.Any(x => x.Email_Address == newContact.Email_Address))
                {
                    MessageBox.Show("Failed to create contact. Contact with Email Address " + newContact.Email_Address + " already exist. Operation aborted.");
                }
                else
                {
                    db.Contact_List.Add(newContact);


                    try
                    {

                        db.SaveChanges();

                       // DataRow dr = new DataRow();
                        object[] dr  = new object[] { newContact.First_Name, newContact.Last_Name, newContact.Email_Address, newContact.Phone_Number, newContact.Title, newContact.Contact_Type, newContact.Decision_Maker, newContact.Budget_Holder, newContact.Department, newContact.Region, newContact.VISN, newContact.Company, newContact.Contract_Coding_Compnay, newContact.City, newContact.State, newContact.AE, newContact.EPS, newContact.Simulator, newContact.VACC, newContact.VACC_R_Notes, newContact.CDI_Pro, newContact.CDI_Pro_R_Notes,  newContact.VERA_Az, newContact.VERA_Az_R_Notes, newContact.OI_T, newContact.NHAC, newContact.Svc_Interrupt_Restore, newContact.E_Bulletin, newContact.Webcast_Bulletin, newContact.iSupport_Account, newContact.Bulletin_Reader, newContact.NPS_Fall_2016, newContact.NPS_Spring_2016, newContact.NPS_Spring_2015, newContact.OI_T_Building_Relationships, newContact.Klas};
                        ds.Tables[0].Rows.Add(dr);

                        MessageBox.Show("Contact Added Successfully");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

            }
            else
            {
                MessageBox.Show("First/Last Name and Email required");
            }
        }

        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                if (e.Button == MouseButtons.Right)
                {
                    this.dataGridView1.Rows[e.RowIndex].Selected = true;
                    this.indexOfRowToDelete = e.RowIndex;
                    this.dataGridView1.CurrentCell = this.dataGridView1.Rows[e.RowIndex].Cells[1];
                    this.ContextMenuStrip_DeleteRow.Show(this.dataGridView1, e.Location);
                    ContextMenuStrip_DeleteRow.Show(Cursor.Position);
                }
            }
        }

        private void ContextMenuStrip_DeleteRow_Click(object sender, EventArgs e)
        {
            string emailOfContactInSearchTab = "";
            string emailToDelete;
            DeleteRowConfirmation deleteForm = new DeleteRowConfirmation();
            deleteForm.ShowDialog();

            //If deleted from tab2, delete from tab 1 also.
            if (deleteForm.AreYouSureYouWantToDeleteThisRow == true)
            {
                if (Tab_Control.SelectedTab == Tab_Search)
                {
                    emailOfContactInSearchTab = dataGridView_SearchResults.Rows[this.indexOfRowToDelete].Cells["Email_Address"].Value.ToString();
                    if (!this.dataGridView_SearchResults.Rows[this.indexOfRowToDelete].IsNewRow)
                    {
                        this.dataGridView_SearchResults.Rows.RemoveAt(this.indexOfRowToDelete);
                    }
                }
            }

            if (deleteForm.AreYouSureYouWantToDeleteThisRow == true)
            {
                if(Tab_Control.SelectedTab == Tab_Search)
                {
                    emailToDelete = emailOfContactInSearchTab;
                }
                else
                {
                    emailToDelete = dataGridView1.Rows[this.indexOfRowToDelete].Cells["Email_Address"].Value.ToString();
                }
                
                var contactToDelete = db.Contact_List.Where(x => x.Email_Address ==  emailToDelete).FirstOrDefault();
                //if current tab is search when deleting, search first tab grid for matching email address and delete it
                if (Tab_Control.SelectedTab == Tab_Search)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells["Email_Address"].Value.ToString() == emailToDelete)
                        {
                            dataGridView1.Rows.RemoveAt(i);
                        }
                    }
                }
                else//If on first tab, just delete item from first tab
                {
                    if (!this.dataGridView1.Rows[this.indexOfRowToDelete].IsNewRow)
                    {
                        this.dataGridView1.Rows.RemoveAt(this.indexOfRowToDelete);

                        //If deleted row is from tab 1, and this row is in tab2 data grid, remove it also.
                        for (int i = 0; i < dataGridView_SearchResults.Rows.Count; i++)
                        {

                            if (dataGridView_SearchResults.Rows[i].Cells["Email_Address"].Value.ToString() == emailToDelete)
                            {
                                dataGridView_SearchResults.Rows.RemoveAt(i);
                            }


                        }
                    }
                }
               
          
                
                try
                {
                    db.Contact_List.Remove(contactToDelete);
                    db.SaveChanges();

                    Label_SearchResults.Text = "";
         
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void dataGridView_SearchResults_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                if (e.Button == MouseButtons.Right)
                {
                    this.dataGridView_SearchResults.Rows[e.RowIndex].Selected = true;
                    this.indexOfRowToDelete = e.RowIndex;
                    this.dataGridView_SearchResults.CurrentCell = this.dataGridView_SearchResults.Rows[e.RowIndex].Cells[1];
                    this.ContextMenuStrip_DeleteRow.Show(this.dataGridView_SearchResults, e.Location);
                    ContextMenuStrip_DeleteRow.Show(Cursor.Position);
                }
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            listDataGridHasChanged = true;
            Button_SaveListEdit.BackColor = Color.Red;
        }

        private void dataGridView_SearchResults_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            searchDataGridHasChanged = true;
            Button_SaveSearchEdit.BackColor = Color.Red;
        }

        private void Button_SaveListEdit_Click(object sender, EventArgs e)
        {
            if (listDataGridHasChanged == true)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(constring))
                    {
                        SqlDataAdapter listDa = new SqlDataAdapter("Select * from Contact_List", con);
                        SqlCommandBuilder cmd = new SqlCommandBuilder(listDa);
                        var cm = dataGridView1.BindingContext[ds.Tables[0]];
                        cm.EndCurrentEdit();


                        con.Open();
                        listDa.Update(ds.Tables[0]);
                        con.Close();

                        listDataGridHasChanged = false;

                        MessageBox.Show("Upates Saved Succesfully");
                        Button_SaveListEdit.BackColor = Color.Transparent;

                        dataGridView_SearchResults.DataSource = null;
                        dataGridView_SearchResults.Rows.Clear();

                        Label_SearchResults.Text = "";



                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No changes to save. Operation Aborted.");
            }
        }
    }
}
