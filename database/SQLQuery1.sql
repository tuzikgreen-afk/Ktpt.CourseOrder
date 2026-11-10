CREATE DATABASE boardgames_shop;
GO

USE boardgames_shop;
GO


CREATE TABLE roles (
    role_id   INT IDENTITY(1, 1),
    role_name NVARCHAR(50) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (role_id),
    CONSTRAINT uq_roles_name UNIQUE (role_name)
);

CREATE TABLE users (
    user_id         INT IDENTITY(1, 1),
    login           NVARCHAR(50) NOT NULL,
    password_hash   NVARCHAR(255) NOT NULL,
    last_name       NVARCHAR(60) NOT NULL,
    first_name      NVARCHAR(60) NOT NULL,
    middle_name     NVARCHAR(60) NULL,
    role_id         INT NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY (user_id),
    CONSTRAINT uq_users_login UNIQUE (login),
    CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles (role_id)
);

CREATE TABLE categories (
    category_id   INT IDENTITY(1, 1),
    category_name NVARCHAR(100) NOT NULL,
    CONSTRAINT pk_categories PRIMARY KEY (category_id),
    CONSTRAINT uq_categories_name UNIQUE (category_name)
);

CREATE TABLE publishers (
    publisher_id   INT IDENTITY(1, 1),
    publisher_name NVARCHAR(120) NOT NULL,
    CONSTRAINT pk_publishers PRIMARY KEY (publisher_id),
    CONSTRAINT uq_publishers_name UNIQUE (publisher_name)
);

CREATE TABLE games (
    game_id         INT IDENTITY(1, 1),
    game_code       NVARCHAR(20) NOT NULL,
    game_name       NVARCHAR(150) NOT NULL,
    category_id     INT NOT NULL,
    publisher_id    INT NOT NULL,
    description     NVARCHAR(MAX) NULL,
    image_path      NVARCHAR(150) NULL,
    base_price      DECIMAL(10, 2) NOT NULL,
    available_stock INT NOT NULL CONSTRAINT df_games_stock DEFAULT (0),
    CONSTRAINT pk_games PRIMARY KEY (game_id),
    CONSTRAINT uq_games_code UNIQUE (game_code),
    CONSTRAINT chk_games_price CHECK (base_price > 0),
    CONSTRAINT chk_games_stock CHECK (available_stock >= 0),
    CONSTRAINT fk_games_category FOREIGN KEY (category_id) REFERENCES categories (category_id),
    CONSTRAINT fk_games_publisher FOREIGN KEY (publisher_id) REFERENCES publishers (publisher_id)
);

CREATE TABLE orders (
    order_id   INT IDENTITY(1, 1),
    order_date DATE NOT NULL CONSTRAINT df_orders_date DEFAULT (CAST(GETDATE() AS DATE)),
    client_id  INT NOT NULL,
    CONSTRAINT pk_orders PRIMARY KEY (order_id),
    CONSTRAINT fk_orders_client FOREIGN KEY (client_id) REFERENCES users (user_id)
);

CREATE TABLE order_items (
    order_item_id INT IDENTITY(1, 1),
    order_id      INT NOT NULL,
    game_id       INT NOT NULL,
    quantity      INT NOT NULL,
    unit_price    DECIMAL(10, 2) NOT NULL,
    CONSTRAINT pk_order_items PRIMARY KEY (order_item_id),
    CONSTRAINT uq_order_game UNIQUE (order_id, game_id),
    CONSTRAINT chk_items_quantity CHECK (quantity > 0),
    CONSTRAINT chk_items_price CHECK (unit_price >= 0),
    CONSTRAINT fk_items_order FOREIGN KEY (order_id) REFERENCES orders (order_id) ON DELETE CASCADE,
    CONSTRAINT fk_items_game FOREIGN KEY (game_id) REFERENCES games (game_id)
);
GO

CREATE INDEX ix_games_category ON games (category_id);
CREATE INDEX ix_games_publisher ON games (publisher_id);
CREATE INDEX ix_orders_date ON orders (order_date);
CREATE INDEX ix_items_game ON order_items (game_id);
GO

INSERT INTO roles (role_name) VALUES 
(N'Гость'), (N'Покупатель'), (N'Сотрудник'), (N'Администратор');

INSERT INTO users (login, password_hash, last_name, first_name, middle_name, role_id) VALUES
(N'admin', N'$2a$11$ExampleHashAdmin123', N'Иванов', N'Иван', N'Иванович', 4),
(N'manager', N'$2a$11$ExampleHashManager456', N'Петров', N'Петр', N'Сергеевич', 3),
(N'client1', N'$2a$11$ExampleHashClient789', N'Сидоров', N'Алексей', N'Михайлович', 2);


INSERT INTO categories (category_name) VALUES 
(N'Стратегии'), (N'Семейные'), (N'Карточные'), (N'Пати-игры');

INSERT INTO publishers (publisher_name) VALUES 
(N'Hobby World'), (N'Свобода Игр'), (N'Crowd Games');

INSERT INTO games (game_code, game_name, category_id, publisher_id, description, image_path, base_price, available_stock) VALUES
(N'BG-001', N'Catan (Колонизаторы)', 1, 1, N'Классическая градостроительная стратегия.', N'catan.png', 3990.00, 12),
(N'BG-002', N'Билет на поезд', 2, 1, N'Увлекательное путешествие на поездах по Америке.', N'ticket.png', 4490.00, 5),
(N'BG-003', N'Взрывные котята', 4, 2, N'Быстрая и веселая карточная игра.', N'kittens.png', 1290.00, 2);

INSERT INTO orders (order_date, client_id) VALUES 
('2026-10-10', 3);

INSERT INTO order_items (order_id, game_id, quantity, unit_price) VALUES 
(1, 1, 1, 3990.00),
(1, 3, 2, 1290.00);
GO
