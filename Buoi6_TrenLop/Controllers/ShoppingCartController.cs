using Buoi6_TrenLop.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Buoi6_TrenLop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private Model1 context = new Model1();
        // GET: ShoppingCart
        public ActionResult Index()
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            if (lstShoppingCart.Count == 0)
                return RedirectToAction("Index", "Book");
            ViewBag.Quatity = lstShoppingCart.Sum(x => x.Quatity);
            ViewBag.Total = lstShoppingCart.Sum(x => x.Price * x.Quatity);
            return View(lstShoppingCart);
        }


        public List<Cart> GetShoppingCartFromSession()
        {
            var lstShoppingCart = Session["ShoppingCart"] as List<Cart>;
            if (lstShoppingCart == null)
            {
                lstShoppingCart = new List<Cart>();
                Session["ShoppingCart"] = lstShoppingCart;
            }
            return lstShoppingCart;
        }
        [Authorize]
        public RedirectToRouteResult AddToCart(int id)
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            Cart findCart = lstShoppingCart.FirstOrDefault(m => m.Id == id);
            if (findCart == null)
            {
                Book findBook = context.Books.FirstOrDefault(m => m.Id == id);
                Cart newItem = new Cart()
                {
                    Id = findBook.Id,
                    Title = findBook.Title,
                    Quatity = 1,
                    Image = findBook.Image,
                    Price = findBook.Price.Value
                };
                lstShoppingCart.Add(newItem);
            }
            else
            {
                findCart.Quatity++;
            }
            return RedirectToAction("Index", "ShoppingCart");
        }

        public RedirectToRouteResult UpdateCart(int id, int txtQuantity)
        {
            var itemFind = GetShoppingCartFromSession().FirstOrDefault(m => m.Id == id);
            if (itemFind != null)
            {
                itemFind.Quatity = txtQuantity;
            }
            return RedirectToAction("Index");

        }
        public ActionResult CartSummary()
        {
            ViewBag.CartCount = GetShoppingCartFromSession().Count();
            return PartialView("CartSummary");
        }
        public ActionResult Order()
        {
            Model1 context = new Model1();
            string currentUserId = User.Identity.GetUserId();
            int newOrderNo;
            using (DbContextTransaction transaction = context.Database.BeginTransaction())
            {
                try
                {
                    Order objOrder = new Order()
                    {
                        CustomerId = currentUserId,
                        OrderDate = DateTime.Now,
                        DeliveryDate = null,
                        isComplete = false,
                        isPaid = false,

                    };
                    context.Orders.Add(objOrder);
                    context.SaveChanges();
                    newOrderNo = context.Database.SqlQuery<int>("SELECT TOP 1 OrderNo FROM [Order] ORDER BY OrderNo DESC").FirstOrDefault();

                    List<Cart> carts = GetShoppingCartFromSession();
                    foreach (var item in carts)
                    {
                        OrderDetail orderDetails = new OrderDetail()
                        {
                            OrderNo = newOrderNo,
                            BookId = item.Id,
                            Quantity = item.Quatity,
                            Price = item.Price
                        };
                        context.OrderDetails.Add(orderDetails);
                        context.SaveChanges();
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return Content("Order Placement Error!" + e.Message);
                }


            }

            return RedirectToAction("ConfirmOrder", "ShoppingCart", new { newOrderNo = newOrderNo });

        }
        public ActionResult ConfirmOrder(int newOrderNo)
        {
            ViewBag.newOrderNo = newOrderNo.ToString();
            return View();
        }
        public ActionResult RemoveCartItem(int? id)
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cartItem = lstShoppingCart.FirstOrDefault(m => m.Id == id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }

            return View(cartItem);
        }
        [HttpPost, ActionName("RemoveCartItem")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            Cart cartItem = lstShoppingCart.FirstOrDefault(m => m.Id == id);
            lstShoppingCart.Remove(cartItem);
            return RedirectToAction("Index");
        }

        public ActionResult Delete()
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string confirmDelete)
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            if (confirmDelete == "true")
            {
                Session["ShoppingCart"] = null;
            }

            return RedirectToAction("Index");
        }




        /////////////////////////////////////////Thanh Toán MOMO///////////////////////////////////////////////// 

        public ActionResult Payment(string id)
        {
            List<Cart> lstShoppingCart = GetShoppingCartFromSession();
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOOJOI20210710";
            string accessKey = "iPXneGmrJH0G8FOP";
            string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
            string orderInfo = "test";
            string returnUrl = "https://d9df-203-205-32-22.ap.ngrok.io/ShoppingCart/ConfirmPaymentClient";
            string notifyurl = "https://d9df-203-205-32-22.ap.ngrok.io/ShoppingCart/ConfirmPaymentClient"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

            string amount = lstShoppingCart.Sum(x => x.Price * x.Quatity).ToString();
            string orderid = id;
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
                                {
                                    { "partnerCode", partnerCode },
                                    { "accessKey", accessKey },
                                    { "requestId", requestId },
                                    { "amount", amount },
                                    { "orderId", orderid },
                                    { "orderInfo", orderInfo },
                                    { "returnUrl", returnUrl },
                                    { "notifyUrl", notifyurl },
                                    { "extraData", extraData },
                                    { "requestType", "captureMoMoWallet" },
                                    { "signature", signature }

                                };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        /*        public ActionResult Payment()
                {

                    //request params need to request to MoMo system
                    string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
                    string partnerCode = "MOMOOJOI20210710";
                    string accessKey = "iPXneGmrJH0G8FOP";
                    string serectkey = "sFcbSGRSJjwGxwhhcEktCHWYUuTuPNDB";
                    string orderInfo = "test";
                    string returnUrl = "https://localhost:8080/ShoppingCart/ConfirmPaymentClient";
                    string notifyurl = "https://d9df-203-205-32-22.ap.ngrok.io/ShoppingCart/ConfirmPaymentClient"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

                    string amount = "161000";
                    string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
                    string requestId = DateTime.Now.Ticks.ToString();
                    string extraData = "";

                    //Before sign HMAC SHA256 signature
                    string rawHash = "partnerCode=" +
                        partnerCode + "&accessKey=" +
                        accessKey + "&requestId=" +
                        requestId + "&amount=" +
                        amount + "&orderId=" +
                        orderid + "&orderInfo=" +
                        orderInfo + "&returnUrl=" +
                        returnUrl + "&notifyUrl=" +
                        notifyurl + "&extraData=" +
                        extraData;

                    MoMoSecurity crypto = new MoMoSecurity();
                    //sign signature SHA256
                    string signature = crypto.signSHA256(rawHash, serectkey);

                    //build body json request
                    JObject message = new JObject
                            {
                                { "partnerCode", partnerCode },
                                { "accessKey", accessKey },
                                { "requestId", requestId },
                                { "amount", amount },
                                { "orderId", orderid },
                                { "orderInfo", orderInfo },
                                { "returnUrl", returnUrl },
                                { "notifyUrl", notifyurl },
                                { "extraData", extraData },
                                { "requestType", "captureMoMoWallet" },
                                { "signature", signature }

                            };

                    string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

                    JObject jmessage = JObject.Parse(responseFromMomo);

                    return Redirect(jmessage.GetValue("payUrl").ToString());
                }*/


        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])

        public ActionResult ConfirmPaymentClient(Models.Result result)
        {
            //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode; // = 0: thanh toán thành công
            Order order = new Order();
            int orderNo = int.Parse(result.orderId);
            order = context.Orders.FirstOrDefault(p => p.OrderNo == orderNo);
            order.isPaid = true;
            order.DeliveryDate = DateTime.Now.AddDays(7);
            context.SaveChanges();
            Session["ShoppingCart"] = null;

            ViewBag.Message = rMessage;
            ViewBag.OrderId = rOrderId;
            ViewBag.ErrorCode = rErrorCode;
            return View();
        }
    }

}