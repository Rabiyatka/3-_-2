using System;
using System.Collections.Generic;
using System.Linq;

namespace Shop
{
    enum Category { Продукты, Электроника, Одежда }

    // Структура для хранения истории продаж
    struct SaleRecord
    {
        public int ProductId;
        public int Quantity;
        public decimal PriceAtSale;
    }

    //Класс Товара
    class Product
    {
        private static int _nextId = 1000; //Генератор уникального кода

        public int Id { get; private set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Category ProdCategory { get; set; }

        //Ищет товар в наличии или нет
        public bool IsInStock => Quantity > 0; //сокращение 

        public Product(string name, decimal price, int quantity, Category category)
        {
            Id = _nextId++;
            Name = name;
            Price = price;
            Quantity = quantity;
            ProdCategory = category;
        }

        public void DisplayInfo()
        {
            string stockStatus = IsInStock ? "В наличии" : "Нет на складе"; // Это крутое сокращение if else, мне понравилось выглядит так будто я круто шарю
            Console.WriteLine($"[{Id}] {Name} | Кат: {ProdCategory} | Цена: {Price:C} | Кол-во: {Quantity} шт. ({stockStatus})");
        }
    }

    class Program
    {
        // Списки для хранения данных
        static List<Product> products = new List<Product>();
        static Stack<SaleRecord> saleHistory = new Stack<SaleRecord>(); // Для отмены продаж
        static List<SaleRecord> allSalesLog = new List<SaleRecord>();  // Для отчета о продажах

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Корректный русский язык в консоли
            SeedData(); // Заполнение тестовыми данными

            while (true)
            {
                Console.WriteLine("\n--- УЧЁТ ТОВАРОВ В МАГАЗИНЕ ---");
                Console.WriteLine("1. Показать все товары       5. Поиск товара");
                Console.WriteLine("2. Добавить новый товар      6. Продать товар");
                Console.WriteLine("3. Удалить товар             7. Отменить последнюю продажу");
                Console.WriteLine("4. Заказать поставку         8. Отчёт о продажах");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите команду: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": ShowAllProducts(); break;
                    case "2": AddNewProduct(); break;
                    case "3": DeleteProduct(); break;
                    case "4": ZakazProduct(); break;
                    case "5": SearchProducts(); break;
                    case "6": SellProduct(); break;
                    case "7": UndoLastSale(); break;
                    case "8": ShowAllSales(); break;
                    case "0": return;
                    default: Console.WriteLine("Ошибка: Неверная команда. Попробуйте снова."); break;
                }
            }
        }

        //ЖесткоеЗаполнениетоварами
        static void SeedData()
        {
            products.Add(new Product("Молоко", 85.50m, 20, Category.Продукты));
            products.Add(new Product("Хлеб", 45.00m, 15, Category.Продукты));
            products.Add(new Product("Смартфон", 29999.00m, 5, Category.Электроника));
            products.Add(new Product("Наушники", 3500.00m, 8, Category.Электроника));
            products.Add(new Product("Футболка", 1200.00m, 12, Category.Одежда));
        }

        static void ShowAllProducts()
        {
            if (products.Count == 0) { Console.WriteLine("Магазин пуст."); return; }
            foreach (var p in products) p.DisplayInfo();
        }

        static void AddNewProduct()
        {
            string name = ReadString("Введите название товара: ");
            decimal price = ReadDecimal("Введите цену товара: ", 0.01m);
            int quantity = ReadInt("Введите начальное количество: ", 0);

            Console.WriteLine("Выберите категорию: 0 - Продукты, 1 - Электроника, 2 - Одежда");
            int categoryID = ReadInt("Номер категории: ", 0, 2);
            Category category = (Category)categoryID;

            products.Add(new Product(name, price, quantity, category));
            Console.WriteLine("Товар добавлен");
        }

        static void DeleteProduct()
        {
            int id = ReadInt("Введите код товара для удаления: ");
            Product p = products.Find(x => x.Id == id);
            if (p != null) { products.Remove(p); Console.WriteLine("Товара нет. товара здесь больше нет."); }
            else Console.WriteLine("Нет товара с таким кодом.");
        }

        static void ZakazProduct()
        {
            int id = ReadInt("Введите код товара для поставки: ");
            Product p = products.Find(x => x.Id == id);
            if (p == null) { Console.WriteLine("Товар не найден."); return; }

            int amount = ReadInt("Какое количество заказать? ", 1);
            p.Quantity += amount;
            Console.WriteLine($"Ну все добавили. Новое количество: {p.Quantity} шт.");
        }

        static void SellProduct()
        {
            int id = ReadInt("Введите код товара для продажи: ");
            Product p = products.Find(x => x.Id == id);
            if (p == null) { Console.WriteLine("Товар не найден."); return; }
            if (!p.IsInStock) { Console.WriteLine("Товара нет на складе!"); return; }

            int qty = ReadInt($"Введите количество для продажи (доступно {p.Quantity} шт.): ", 1);
            if (qty > p.Quantity) { Console.WriteLine("Ошибка. Нет столько добра. "); return; }

            p.Quantity -= qty;

            //Я Фиксирую я фиксирую , я тоже фиксирую, фиксирую продажу для истории и отчетов
            SaleRecord sale = new SaleRecord { ProductId = p.Id, Quantity = qty, PriceAtSale = p.Price };
            saleHistory.Push(sale);
            allSalesLog.Add(sale);

            Console.WriteLine($"Продано. Общая стоимость: {qty * p.Price:C}");
        }

        static void UndoLastSale()
        {
            if (saleHistory.Count == 0) { Console.WriteLine("История продаж пуста. Нечего отменять."); return; } 

            SaleRecord lastSale = saleHistory.Pop();
            allSalesLog.Remove(lastSale); // Удаляем из общего отчета

            Product p = products.Find(x => x.Id == lastSale.ProductId);
            if (p != null) p.Quantity += lastSale.Quantity; // Возвращаем товар на склад

            Console.WriteLine("Галя, отмена. Товар возвращен на склад.");
        }

        static void ShowAllSales()
        {
            Console.WriteLine("--- ОТЧЁТ О ПРОДАЖАХ ---");
            if (allSalesLog.Count == 0) { Console.WriteLine("Продаж еще не было."); return; }

            decimal totalRevenue = 0;
            foreach (var sale in allSalesLog)
            {
                Product p = products.Find(x => x.Id == sale.ProductId);
                string prodName = p != null ? p.Name : "[Удаленный товар]";
                decimal sum = sale.Quantity * sale.PriceAtSale;
                totalRevenue += sum;
                Console.WriteLine($"- {prodName} (Код: {sale.ProductId}) | {sale.Quantity} шт. x {sale.PriceAtSale:C} = {sum:C}");
            }
            Console.WriteLine($" Выручка получается: {totalRevenue:C}");
        }

        static void SearchProducts()
        {
            Console.WriteLine("Как искать? 1 - По коду, 2 - По названию, 3 - По категории");
            string knopka = Console.ReadLine();

            if (knopka == "1")
            {
                int id = ReadInt("Введите код: ");
                var res = products.Where(x => x.Id == id);
                PrintResults(res);
            }
            else if (knopka == "2")
            {
                Console.Write("Введите часть названия: ");
                string name = Console.ReadLine().ToLower();
                var res = products.Where(x => x.Name.ToLower().Contains(name));
                PrintResults(res);
            }
            else if (knopka == "3")
            {
                Console.WriteLine("0 - Продукты, 1 - Электроника, 2 - Одежда");
                int cat = ReadInt("Выберите категорию: ", 0, 2);
                var res = products.Where(x => x.ProdCategory == (Category)cat);
                PrintResults(res);
            }
            else Console.WriteLine("Неверный режим поиска.");
        }

        static void PrintResults(IEnumerable<Product> results)
        {
            if (!results.Any()) Console.WriteLine("Товары не найдены.");
            foreach (var p in results) p.DisplayInfo();
        }

        //Чтоб не вылетала птичка
        static string ReadString(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(input)) return input;
                Console.WriteLine("Одна ошибка и вы ошиблись. Строка не может быть пустой.");
            }
        }

        static int ReadInt(string message, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out int result) && result >= min && result <= max) return result;
                Console.WriteLine($"Ошибка. Введите целое число в диапазоне от {min} до {max}.");
            }
        }

        static decimal ReadDecimal(string message, decimal min = 0)
        {
            while (true)
            {
                Console.Write(message);
                if (decimal.TryParse(Console.ReadLine(), out decimal result) && result >= min) return result; 
                    Console.WriteLine($"Ошибка: Введите положительное число (минимум {min}).");
            }
        }
    }
}