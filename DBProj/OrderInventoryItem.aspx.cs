﻿using System;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DBProj
{
    public partial class OrderInventoryItem : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateInventoryItemsDropDown();
            }
        }

        private void PopulateInventoryItemsDropDown()
        {
            string connectionString = "Data Source=AHSANELITEBOOK\\SQLEXPRESS;Initial Catalog=WebApp;Integrated Security=True";
            string query = "SELECT ItemID, Name FROM Inventory";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                ddlInventoryItems.DataSource = reader;
                ddlInventoryItems.DataTextField = "Name";
                ddlInventoryItems.DataValueField = "ItemID";
                ddlInventoryItems.DataBind();

                reader.Close();
            }

            ddlInventoryItems.Items.Insert(0, new ListItem("--Select Item--", "0"));
        }

        protected void OrderInventoryItem_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=AHSANELITEBOOK\\SQLEXPRESS;Initial Catalog=WebApp;Integrated Security=True";

            int itemID = int.Parse(ddlInventoryItems.SelectedValue);
            int quantity = Convert.ToInt32(quantityOrdered.Value);
            int price = 0;
            if (!string.IsNullOrEmpty(Request.Form["price"]))
            {
                price = Convert.ToInt32(Request.Form["price"]); // Correctly retrieving the price
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Retrieve the name of the selected item
                    string itemNameQuery = "SELECT Name FROM Inventory WHERE ItemID = @itemID";
                    string itemName = "";
                    using (SqlCommand cmdName = new SqlCommand(itemNameQuery, conn))
                    {
                        cmdName.Parameters.AddWithValue("@itemID", itemID);
                        itemName = (string)cmdName.ExecuteScalar();
                    }

                    // Insert the order details into InventoryStockOrders
                    string query = "INSERT INTO InventoryStockOrders (ItemID, Name, QuantityOrdered, Price, OrderDate) VALUES (@itemID, @name, @quantity, @price, @orderDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@itemID", itemID);
                        cmd.Parameters.AddWithValue("@name", itemName);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@orderDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Clear form fields and show success message
                ddlInventoryItems.SelectedIndex = 0;
                quantityOrdered.Value = "";
                //price.Text = ""; // Clearing the price field

                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "alert('Inventory Item Ordered');", true);
            }
            catch (Exception ex)
            {
                Response.Write("Error: " + ex.Message);
            }
        }
    }
}
