-- up
CREATE TABLE IF NOT EXISTS products (
 id INTEGER NOT NULL,
 name TEXT NULL,
 price NUMERIC NOT NULL,
 discount NUMERIC NULL,
 instock BOOLEAN NOT NULL
);

-- down
DROP TABLE IF EXISTS products;
