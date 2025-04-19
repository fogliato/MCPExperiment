
-- E-Commerce MySQL Schema and Sample Data
-- Generated at 2025-04-19 13:50:21

-- Drop and create database
DROP DATABASE IF EXISTS ecommerce;
CREATE DATABASE ecommerce;
USE ecommerce;

-- Drop tables (respect foreign key dependencies)
DROP TABLE IF EXISTS payment_info;
DROP TABLE IF EXISTS shipping_info;
DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS inventory;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS customers;

-- Customers
CREATE TABLE customers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Categories
CREATE TABLE categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT
);

-- Products
CREATE TABLE products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(10,2) NOT NULL,
    category_id INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories(id)
);

-- Inventory
CREATE TABLE inventory (
    product_id INT PRIMARY KEY,
    quantity INT NOT NULL DEFAULT 0,
    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id)
);

-- Orders
CREATE TABLE orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    order_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    status ENUM('pending', 'paid', 'shipped', 'delivered', 'cancelled') DEFAULT 'pending',
    total_amount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);

-- Order Items
CREATE TABLE order_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);

-- Shipping Info
CREATE TABLE shipping_info (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    address_line1 VARCHAR(255) NOT NULL,
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    zip_code VARCHAR(20),
    country VARCHAR(100),
    shipping_method VARCHAR(50),
    shipping_cost DECIMAL(10,2),
    shipped_at DATETIME,
    FOREIGN KEY (order_id) REFERENCES orders(id)
);

-- Payment Info
CREATE TABLE payment_info (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    payment_method VARCHAR(50),
    payment_status ENUM('pending', 'completed', 'failed', 'refunded') DEFAULT 'pending',
    paid_at DATETIME,
    transaction_id VARCHAR(255),
    FOREIGN KEY (order_id) REFERENCES orders(id)
);

-- Sample Inserts

INSERT INTO customers (first_name, last_name, email, password_hash, phone)
VALUES 
('Alice', 'Smith', 'alice@example.com', 'hashed_password1', '1234567890'),
('Bob', 'Johnson', 'bob@example.com', 'hashed_password2', '0987654321');

INSERT INTO categories (name, description)
VALUES 
('Electronics', 'Devices and gadgets'),
('Books', 'Printed and digital books'),
('Clothing', 'Apparel and accessories');

INSERT INTO products (name, description, price, category_id)
VALUES 
('Smartphone', 'Latest model smartphone', 699.99, 1),
('Laptop', 'High performance laptop', 1299.00, 1),
('Novel - Sci-Fi', 'A science fiction novel', 19.99, 2),
('T-Shirt', 'Cotton T-shirt', 15.00, 3);

INSERT INTO inventory (product_id, quantity)
VALUES 
(1, 50),
(2, 20),
(3, 100),
(4, 75);

INSERT INTO orders (customer_id, order_date, status, total_amount)
VALUES 
(1, NOW(), 'paid', 714.99),
(2, NOW(), 'pending', 34.99);

INSERT INTO order_items (order_id, product_id, quantity, price)
VALUES 
(1, 1, 1, 699.99),
(1, 4, 1, 15.00),
(2, 3, 1, 19.99),
(2, 4, 1, 15.00);

INSERT INTO shipping_info (order_id, address_line1, city, state, zip_code, country, shipping_method, shipping_cost)
VALUES 
(1, '123 Main St', 'New York', 'NY', '10001', 'USA', 'Standard', 5.00),
(2, '456 Elm St', 'San Francisco', 'CA', '94105', 'USA', 'Express', 10.00);

INSERT INTO payment_info (order_id, payment_method, payment_status, paid_at, transaction_id)
VALUES 
(1, 'Credit Card', 'completed', NOW(), 'TX123456789'),
(2, 'PayPal', 'pending', NULL, NULL);
