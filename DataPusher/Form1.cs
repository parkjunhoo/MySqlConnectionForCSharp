using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using static DataPusher.Form1;

namespace DataPusher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class Product
        {
            public int product_id;
            public int product_cate_id;
            public int product_brand_id;
            public string product_eng_name;
            public string product_kor_name;
            public string product_release;
            public string product_color;
            public string product_model_num;
            public DateTime product_date;
            public bool product_state;
        }
        public class ProductBrand
        {
            public int product_brand_id;
            public string product_brand_eng_name;
            public string product_brand_kor_name;
            public string product_brand_img;
        }
        public class ProductCate
        {
            public int product_cate_id;
            public string product_cate_name;
        }
        public class ProductSize
        {
            public int product_size_id;
            public int product_id;
            public string product_size_value;
        }
        public class ImgThumb
        {
            public int img_thumb_id;
            public int comm_no;
            public int product_id;
            public string img_thumb_url;
            public string img_thumb_store;
        }


        public class ProductJson
        {
            public string productCateName;
            public string productEngName;
            public string productKorName;
            public string productRelease;
            public string productBrandEngName;
            public string productBrandKorName;
            public string productBrandImgUrl;
            public string productColor;
            public string productModelNum;
            public string productDate;
            public bool productState;
            public List<string> thumbLinks;
            public List<string> sizeList;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "jso");
            string jsonPath = Path.Combine(folderPath, "merged.json");
            string jsonString = File.ReadAllText(jsonPath);
            JArray jarr = JArray.Parse(jsonString);

            List<ProductJson> jsonProductList = JsonConvert.DeserializeObject<List<ProductJson>>(jsonString);

            List<Product> productList = new List<Product>();
            List<ProductBrand> brandList = new List<ProductBrand>();
            List<ProductSize> sizeList = new List<ProductSize>();
            List<ProductCate> cateList = new List<ProductCate>();
            List<ImgThumb> imgList = new List<ImgThumb>();


            int idx = 1;
            foreach(ProductJson pj in jsonProductList)
            {
                Product p = new Product();
                p.product_id = idx;
                p.product_eng_name = pj.productEngName.Replace("'","\'\'");
                p.product_kor_name = pj.productKorName.Replace("'", "\'\'");
                p.product_release = pj.productRelease.Replace(",","");
                p.product_color = pj.productColor;
                p.product_model_num = pj.productModelNum;
                if(DateTime.TryParseExact(pj.productDate, "yy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    p.product_date = result;
                }
                p.product_state = true;


                
                ProductCate cate = cateList.FirstOrDefault(c => c.product_cate_name == pj.productCateName);
                if(cate == null)
                {
                    cate = new ProductCate();
                    cate.product_cate_id = cateList.Count+1;
                    cate.product_cate_name = pj.productCateName;
                    cateList.Add(cate);
                }
                p.product_cate_id = cate.product_cate_id;

                ProductBrand brand = brandList.FirstOrDefault(b => b.product_brand_eng_name == pj.productBrandEngName);
                if(brand == null)
                {
                    brand = new ProductBrand();
                    brand.product_brand_id = brandList.Count+1;
                    brand.product_brand_eng_name = pj.productBrandEngName.Replace("'", "\'\'");
                    brand.product_brand_kor_name = pj.productBrandKorName.Replace("'", "\'\'");
                    brand.product_brand_img = pj.productBrandImgUrl;
                    brandList.Add(brand);
                }
                p.product_brand_id = brand.product_brand_id;

                foreach(string s in pj.sizeList)
                {
                    if (s.Equals("¸ðµç »çÀÌÁî")) continue;
                    ProductSize size = new ProductSize();
                    size.product_size_id = sizeList.Count + 1;
                    size.product_id = p.product_id;
                    size.product_size_value = s;
                    sizeList.Add(size);
                }

                foreach(string i in pj.thumbLinks)
                {
                    ImgThumb img = new ImgThumb();
                    img.img_thumb_id = imgList.Count + 1;
                    img.product_id = p.product_id;
                    img.img_thumb_store = "";
                    img.img_thumb_url = i;
                    imgList.Add(img);
                }
                productList.Add(p);
                Console.WriteLine("product List »ý¼º" + p.product_id);
                idx++;
            }




            string connectionString = "Server=localhost;Port=3306;Database=stylehive;Uid=root;Pwd=1234;";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    foreach (Product product in productList)
                    {
                        string productInsertQuery = $"INSERT INTO product (product_id, product_cate_id, product_brand_id, product_eng_name, product_kor_name, product_release, product_color, product_model_num, product_date, product_state) " +
                            $"VALUES ('{product.product_id}', '{product.product_cate_id}', '{product.product_brand_id}', '{product.product_eng_name}', '{product.product_kor_name}', '{product.product_release}', '{product.product_color}', '{product.product_model_num}', '{product.product_date.ToString("yyyy-MM-dd")}', '{(product.product_state ? 1 : 0)}')";

                        MySqlCommand productCommand = new MySqlCommand(productInsertQuery, con);
                        productCommand.ExecuteNonQuery();
                        Console.WriteLine("product »ðÀÔ" + product.product_id);
                        Console.WriteLine("Äõ¸®:" + productInsertQuery);
                    }

                    foreach (ProductBrand brand in brandList)
                    {
                        string brandInsertQuery = $"INSERT INTO product_brand (product_brand_id, product_brand_eng_name, product_brand_kor_name, product_brand_img) " +
                            $"VALUES ('{brand.product_brand_id}', '{brand.product_brand_eng_name}', '{brand.product_brand_kor_name}', '{brand.product_brand_img}')";

                        MySqlCommand brandCommand = new MySqlCommand(brandInsertQuery, con);
                        brandCommand.ExecuteNonQuery();
                        Console.WriteLine("brand »ðÀÔ" + brand.product_brand_id);
                    }
                    /*
                    foreach (ProductSize size in sizeList)
                    {
                        string sizeInsertQuery = $"INSERT INTO product_size (product_size_id, product_id, product_size_value) " +
                            $"VALUES ('{size.product_size_id}', '{size.product_id}', '{size.product_size_value}')";

                        MySqlCommand sizeCommand = new MySqlCommand(sizeInsertQuery, con);
                        sizeCommand.ExecuteNonQuery();
                    }
                    */

                    /*
                    foreach (ProductCate cate in cateList)
                    {
                        string cateInsertQuery = $"INSERT INTO product_cate (product_cate_id, product_cate_name) " +
                            $"VALUES ('{cate.product_cate_id}', '{cate.product_cate_name}')";

                        MySqlCommand cateCommand = new MySqlCommand(cateInsertQuery, con);
                        cateCommand.ExecuteNonQuery();
                    }
                    */

                    /*
                    foreach (ImgThumb img in imgList)
                    {
                        string imgInsertQuery = $"INSERT INTO img_thumb (img_thumb_id, product_id, img_thumb_url, img_thumb_store) " +
                            $"VALUES ('{img.img_thumb_id}', '{img.product_id}', '{img.img_thumb_url}', '{img.img_thumb_store}')";

                        MySqlCommand imgCommand = new MySqlCommand(imgInsertQuery, con);
                        imgCommand.ExecuteNonQuery();
                    }
                    */

                    Console.WriteLine("All is done");
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error:" + ex.Message);
                }

            }
        }

    }
}