﻿using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class BasketService : IBasketService
    {
        private readonly IRepository<Basket> _basketRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<BasketItem> _basketItemRepo;

        public BasketService(IRepository<Basket> basketRepo, IRepository<Product> productRepo, IRepository<BasketItem> basketItemRepo)
        {
            _basketRepo = basketRepo;
            _productRepo = productRepo;
            _basketItemRepo = basketItemRepo;
        }

        public async Task<Basket> AddItemToBasketAsync(int basketId, int productId, int quantity)
        {
            if (quantity < 1)
                throw new ArgumentException("Quantity must be grater than zero");
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) 
                throw new ArgumentException($"Product with the id {productId} cannot be found.");
            var spec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepo.FirstOrDefaultAsync(spec);
            if (basket == null) 
                throw new ArgumentException($"Basket with the id {basketId} cannot be found.");

            var basketItem = basket.Items.FirstOrDefault(x => x.ProductId == productId);
            if (basketItem == null)
            { 
                basketItem = new BasketItem() { BasketId = basketId, ProductId = productId, Quantity = quantity, Product = product};
                basket.Items.Add(basketItem);
            }
            else
                basketItem.Quantity += quantity;
            await _basketRepo.UpdateAsync(basket);
            
            return basket;
        }

        public async Task DeleteBasketAsync(int basketId)
        {
            var basket = await _basketRepo.GetByIdAsync(basketId);
            if (basket == null)
                throw new ArgumentException($"Basket with the id {basket} cannot be found.");
            await _basketRepo.DeleteAsync(basket);
        }

        public async Task DeleteBasketItemAsync(int basketId, int basketItemId)
        {
            var basketItem = await _basketItemRepo.GetByIdAsync(basketItemId);

            if (basketItem == null || basketItem.BasketId != basketId)
                throw new ArgumentException($"Basket item cannot be found.");
            await _basketItemRepo.DeleteAsync(basketItem);
        }

        public async Task<Basket> SetQuantitiesAsync(int basketId, Dictionary<int, int> quantities)
        {
            if (quantities.Values.Any(x => x < 1))
                throw new ArgumentException("Quantity must be grater than zero");
            var spec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepo.FirstOrDefaultAsync(spec);
            if (basket == null)
                throw new ArgumentException($"Basket with the id {basketId} cannot be found.");

            foreach (var item in basket.Items)
            {
                if (quantities.ContainsKey(item.ProductId))
                {
                    item.Quantity = quantities[item.ProductId];
                }
            }

            await _basketRepo.UpdateAsync(basket);

            return basket;
        }

        public async Task TransferBasketAsync(string anonymousId, string userId)
        {
            var specAnon = new BasketWithItemsSpecification(anonymousId);
            var anonBasket = await _basketRepo.FirstOrDefaultAsync(specAnon);

            if (anonBasket == null)
                return;
            
            var specUser = new BasketWithItemsSpecification(userId);
            var userBasket = await _basketRepo.FirstOrDefaultAsync(specUser);

            if (userBasket == null)
            {
                userBasket = new Basket() { BuyerId = userId };
                await _basketRepo.AddAsync(userBasket);
            }

            foreach (BasketItem item in anonBasket.Items)
            {
                BasketItem targetItem = userBasket.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (targetItem != null)
                    targetItem.Quantity += item.Quantity;
                else
                    userBasket.Items.Add(new BasketItem() { ProductId = item.ProductId, BasketId = item.BasketId, Quantity = item.Quantity});

            }
                await _basketRepo.UpdateAsync(userBasket);
                await _basketRepo.DeleteAsync(anonBasket);
        }
    }
}
