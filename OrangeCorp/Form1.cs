using Newtonsoft.Json;
using System.Drawing.Imaging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;

namespace OrangeCorp
{
    public partial class wack_form : Form
    {
        private Pen pen = new Pen(Color.Black);
        private Color defaultShapeColor = Color.Black;
        private Color currentShapeColor;
        private bool colorChanged = false;

        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto            
        };

        public wack_form()
        {
            InitializeComponent();
            DoubleBuffered = true; //minskar risken att rita ytan flimrar när det ritas, var tänkt användas för att rita streck men kom inte så långt.
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //bools för vardera form för att styra vilken som används och iså fall sätta resterande till false.
        private bool drawCircle = false;
        private bool drawSquare = false;
        private bool drawRectangle = false;
        private bool drawTriangle = false;

        //använder stack för att kunna "ångra" och "ångra-ångra" det vi ritat ut på skärmen.
        //I stack kan man enkelt ta bort det senaste inlagda elementet i "högen".
        private Stack<Shape> shapes = [];
        private Stack<Shape> redo = [];

        public TypeNameHandling TypeNameHandling { get; private set; }


        //våran pictureBox där vi beroende på vilken checkbox vi kryssat i ritar ut vald figur med vald färg. 
        private void pb_box_Click(object sender, EventArgs e)
        {
            var mouse = (MouseEventArgs)e;
            Color shapeColor = currentShapeColor;

            if (drawCircle)
            {
                Circle circle = new Circle()
                {
                    Color = shapeColor,
                    Center = mouse.Location,
                    Radius = 100
                };
                shapes.Push(circle);
                redo.Clear();


            }
            else if (drawSquare)
            {
                Square square = new Square()
                {
                    Color = shapeColor,
                    Center = mouse.Location,
                    side = 20,
                };
                shapes.Push(square);
                redo.Clear();
            }

            else if (drawRectangle)
            {
                RectangleShape rectangle = new RectangleShape()
                {
                    Color = shapeColor,
                    height = 20,
                    width = 50,
                    Center = mouse.Location,
                };
                shapes.Push(rectangle);
                redo.Clear();
            }
            else if (drawTriangle)
            {
                int x = mouse.X;
                int y = mouse.Y;
                Triangle triangle = new Triangle()
                {
                    Color = shapeColor,
                    points = new Point[] { new(x + 100, y), new(x - 100, y), new(x, y - 200) },
                    Center = mouse.Location
                };
                shapes.Push(triangle);
                redo.Clear();
            }
            pb_box.Refresh();
        }


        private void pb_box_Paint(object sender, PaintEventArgs e)
        {
            foreach (var s in shapes)
            {
                pen.Color = s.Color;

                if (s is Circle c)
                {
                    e.Graphics.DrawEllipse(pen, c.Center.X - c.Radius / 2, c.Center.Y - c.Radius / 2, c.Radius, c.Radius);
                }
                else if (s is Square sq)
                {
                    e.Graphics.DrawRectangle(pen, sq.Center.X, sq.Center.Y, sq.side, sq.side);
                }
                else if (s is RectangleShape r)
                {
                    e.Graphics.DrawRectangle(pen, r.Center.X, r.Center.Y, r.width, r.height);
                }
                else if (s is Triangle t)
                {
                    e.Graphics.DrawPolygon(pen, t.points);
                }

            }

        }

        //Vi tömmer shapes på eventuella utritade figurer och updaterar våran picturebox
        private void btn_clear_Click(object sender, EventArgs e)
        {
            shapes.Clear();
            pb_box.Refresh();
        }

        private void cb_circle_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_circle.Checked)
            {
                drawCircle = true;
                cb_square.Checked = false;
                cb_Rectangle.Checked = false;
                cb_triangle.Checked = false;
            }
            else
            {
                drawCircle = false;
            }
        }

        private void cb_square_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_square.Checked)
            {
                drawSquare = true;
                cb_circle.Checked = false;
                cb_Rectangle.Checked = false;
                cb_triangle.Checked = false;
            }
            else
            {
                drawSquare = false;
            }
        }

        private void cb_Rectangle_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Rectangle.Checked)
            {
                drawRectangle = true;
                cb_circle.Checked = false;
                cb_square.Checked = false;
                cb_triangle.Checked = false;
            }
            else
            {
                drawRectangle = false;
            }
        }

        private void cb_triangle_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_triangle.Checked)
            {
                drawTriangle = true;
                cb_circle.Checked = false;
                cb_Rectangle.Checked = false;
                cb_square.Checked = false;
            }
            else
            {
                drawTriangle = false;
            }
        }



        //Spara metod för att spara ner utritade figurer till en Jsonfil som vi sedan kan öppna upp igen.
        private void bt_save_Click(object sender, EventArgs e)
        {

            var result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                var json = JsonConvert.SerializeObject(shapes, Formatting.Indented, jsonSettings);

                File.WriteAllText(saveFileDialog1.FileName, json);
            }
        }
        private void bt_load_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                var file = openFileDialog1.FileName;
                var content = File.ReadAllText(file);

                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                shapes = JsonConvert.DeserializeObject<Stack<Shape>>(content, settings);

                var tempShapesList = new List<Shape>();

                pb_box.Refresh();
            }
        }

        //Flyttar element mellan våra stacks för att kunna ångra och sen ångra tillbaks mellan stackarna med våra utritade figurer
        private void bt_undo_Click(object sender, EventArgs e)
        {

            switchElement(shapes, redo);

        }

        private void bt_redo_Click(object sender, EventArgs e)
        {
            switchElement(redo, shapes);
        }

        private void switchElement(Stack<Shape> giver, Stack<Shape> receiver)
        {
            if (giver.Any())
            {
                receiver.Push(giver.Pop());
                pb_box.Refresh();
            }
        }

        //Färgpalett för att välja mellan olika färger. Vi hämtar bakrundsfärgene från vald pictureBox som sätts till våran pen.
        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            currentShapeColor = p.BackColor;
            pen.Color = currentShapeColor;
            pb_ChosenColor.BackColor = currentShapeColor;
            colorChanged = true;

        }

        //Metoder för att spara ner ritade filen till olika bildformat.
        private void bt_saveFile_Click(object sender, EventArgs e)
        {

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG (.png)|.png|JPEG (.jpg;.jpeg)|.jpeg|BMP (.bmp)|.bmp|All files (.)|.*";
            saveFileDialog.DefaultExt = "";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SavePictureBoxToImage(saveFileDialog.FileName);
            }
        }

        private void SavePictureBoxToImage(string filename)
        {
            if (pb_box == null && pb_box.Width == 0 && pb_box.Height == 0)
               return;

            using (var bitmap = new Bitmap(pb_box.Width, pb_box.Height))
            {
                pb_box.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

                using (Graphics g = Graphics.FromImage(bitmap))
                {

                }

                ImageFormat format = ImageFormat.Png;
                string extension = Path.GetExtension(filename).ToLowerInvariant();
                switch (extension)
                {
                    case ".png":
                        format = ImageFormat.Png;
                        break;
                    case ".jpg":
                    case ".jpeg":
                        format =ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }

                bitmap.Save(filename, format);
            }
        }
    }
}

