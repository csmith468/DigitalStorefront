USE DigitalStorefront;
GO

-- Insert Product Types
INSERT INTO dbo.productType (typeName, typeCode, description) VALUES
  ('Pet', 'pet', 'Virtual pets for adoption'),
  ('Furniture', 'furniture', 'Room decorations and furniture'),
  ('Clothing', 'clothing', 'Outfits and accessories'),
  ('Food', 'food', 'Pet food and treats'),
  ('Bundle', 'bundle', 'Product bundles and packages');
GO

-- Insert Category
INSERT INTO dbo.category (name, slug, displayOrder) VALUES 
  ('Virtual Pets', 'virtual-pets', 1),
  ('Seasonal', 'seasonal', 2),
  ('Pet Clothing', 'pet-clothing', 3),
  ('Room Themes', 'room-themes', 4),
  ('Room Items', 'room-items', 5);
GO

-- Insert Price Type
INSERT INTO dbo.priceType (priceTypeName, priceTypeCode) VALUES
  ('Coins', 'coins'),
  ('US Dollars', 'usd');
GO

-- Insert Subcategory with inline category lookups
INSERT INTO dbo.subcategory (categoryId, name, slug, displayOrder) 
SELECT c.categoryId, s.name, s.slug, s.displayOrder
FROM (VALUES
  -- Virtual Pets subcategory
  ('Virtual Pets', 'Regular Pets', 'regular-pets', 1),
  ('Virtual Pets', 'Mythical Pets', 'mythical-pets', 2),
  ('Virtual Pets', 'Little Pets', 'little-pets', 3),
  ('Virtual Pets', 'Aquatic Pets', 'aquatic-pets', 4),
  -- Seasonal subcategory
  ('Seasonal', 'Fall Room Items', 'fall-room-items', 1),
  ('Seasonal', 'Fall Dispensers', 'fall-dispensers', 2),
  ('Seasonal', 'Fall Clothes', 'fall-clothes', 3),
  -- Pet Clothing subcategory
  ('Pet Clothing', 'Tops', 'tops', 1),
  ('Pet Clothing', 'Bottoms & Skirts', 'bottoms-skirts', 2),
  ('Pet Clothing', 'Shoes', 'shoes', 3),
  ('Pet Clothing', 'Accessories', 'accessories', 4),
  -- Room Themes subcategory
  ('Room Themes', 'Beach', 'beach-theme', 1),
  ('Room Themes', 'Farmhouse', 'farmhouse-theme', 2),
  ('Room Themes', 'Fruity', 'fruity-theme', 3),
  ('Room Themes', 'Pumpkin Patch', 'pumpkin-patch-theme', 4),
  ('Room Themes', 'Sleepover', 'sleepover-theme', 5),
  -- Room Items subcategory
  ('Room Items', 'Wallpaper & Flooring', 'wallpaper-flooring', 1),
  ('Room Items', 'Seats', 'seats', 2),
  ('Room Items', 'Kitchen', 'kitchen', 3),
  ('Room Items', 'Dispensers', 'dispensers', 4)
) AS s(categoryName, name, slug, displayOrder)
JOIN dbo.category c ON c.name = s.categoryName;
GO

-- Create temporary table for products to simplify insertion
DECLARE @ProductData TABLE (
  name NVARCHAR(200),
  slug NVARCHAR(200),
  productType NVARCHAR(20),
  priceType NVARCHAR(10),
  price DECIMAL(10,2),
  premiumPrice DECIMAL(10,2),
  isTradeable BIT,
  description NVARCHAR(MAX)
);

-- Insert all product data
INSERT INTO @ProductData VALUES
  -- Virtual Pets (pet type, coins)
  ('Cocoa Corgi', 'cocoa-corgi', 'pet', 'coins', 10000, 8000, 0, 'This adorable short-legged pup loves to waddle around your room and perform silly tricks! With fluffy fur and a perpetual smile, this pup brings joy to every virtual playdate.'),
  ('Stardust Dragon', 'stardust-dragon', 'pet', 'coins', 12000, 10000, 0, 'A shimmering purple dragon with constellation patterns on its wings. This magical companion breathes sparkles instead of fire.'),
  ('Pocket Hedgehog', 'pocket-hedgehog', 'pet', 'usd', 10.99, 8.99, 0, 'Tiny but mighty! This palm-sized hedgehog makes soft purring sounds when happy.'),
  ('Rainbow Octopus', 'rainbow-octopus', 'pet', 'coins', 10000, 8000, 0, 'This color-changing octopus creates beautiful ink art in its underwater room! Watch as it uses all eight tentacles to juggle shells and play underwater instruments.'),
  
  -- Pet Clothing (clothing type, coins)
  ('Star Sweater', 'star-sweater', 'clothing', 'coins', 2000, 1500, 0, 'A cozy knit sweater with multi-colored stars! Perfect for an evening stroll in the park.'),
  ('Cargo Shorts', 'cargo-shorts', 'clothing', 'coins', 2000, 1500, 0, 'Equipped with pockets big enough to hold all your favorite treats! These khaki shorts are perfect for pets who love exploring and collecting treasures.'),
  ('Cloud Sneakers', 'cloud-sneakers', 'clothing', 'coins', 1500, 1000, 0, 'These puffy white sneakers let your pet bounce extra high and leave little cloud puffs with each step. Great for athletic activities!'),
  ('Red Rocker Collar', 'red-rocker-collar', 'clothing', 'coins', 1500, 1000, 0, 'This red collar shows your pet is really into rock and roll!'),
  
  -- Fall Clothing Items (clothing type, coins)
  ('Harvest Moon Hoodie', 'harvest-moon-hoodie', 'clothing', 'coins', 2500, 2000, 0, 'A cozy burnt orange hoodie featuring a glowing moon design with falling autumn leaves. The front pocket is shaped like a basket for collecting acorns.'),
  ('Maple Leaf Sweater', 'maple-leaf-sweater', 'clothing', 'coins', 2200, 1800, 0, 'A cream-colored cable knit sweater with red and gold maple leaves embroidered across the chest. The leaves shimmer and appear to float when your pet moves.'),
  ('Scarecrow Flannel', 'scarecrow-flannel', 'clothing', 'coins', 2000, 1600, 0, 'A red and black checkered flannel shirt with patches on the elbows and straw peeking out from the cuffs, perfect for apple picking adventures.'),
  ('Pumpkin Patch Overalls', 'pumpkin-patch-overalls', 'clothing', 'coins', 2300, 1900, 0, 'Denim overalls with orange pumpkin patches on the knees and a embroidered vine pattern climbing up one leg. Features extra pockets for storing fall treasures.'),
  ('Autumn Plaid Skirt', 'autumn-plaid-skirt', 'clothing', 'coins', 1800, 1400, 0, 'A twirly skirt in warm browns, oranges, and yellows with a leaf-print hem that rustles when your pet walks.'),
  ('Cider Mill Pants', 'cider-mill-pants', 'clothing', 'coins', 2000, 1600, 0, 'Corduroy pants in deep cinnamon brown with apple-shaped buttons and cuffs that can be rolled up to reveal a gingham pattern.'),
  ('Leaf Pile Boots', 'leaf-pile-boots', 'clothing', 'coins', 1700, 1300, 0, 'Tall rain boots that look like your pet is walking through crunchy autumn leaves. Each step shows colorful leaves stuck to the soles.'),
  ('Acorn Cap Slippers', 'acorn-cap-slippers', 'clothing', 'coins', 1200, 900, 0, 'Cozy brown slippers shaped like acorn caps with fuzzy wool lining, perfect for indoor autumn days.'),
  ('Harvest Festival Scarf', 'harvest-festival-scarf', 'clothing', 'coins', 1000, 750, 0, 'A long knitted scarf that gradients from green to yellow to orange to red, mimicking the changing fall foliage.'),
  ('Turkey Trot Hat', 'turkey-trot-hat', 'clothing', 'coins', 1500, 1200, 0, 'A pilgrim-style hat with a turkey feather tucked in the band that bobbles when your pet moves.'),
  ('Apple Cider Mittens', 'apple-cider-mittens', 'clothing', 'coins', 800, 600, 0, 'Soft red mittens with green stems on top to look like apples, complete with a cinnamon stick charm dangling from each wrist.'),
  ('Cornucopia Backpack', 'cornucopia-backpack', 'clothing', 'coins', 2000, 1600, 0, 'A woven horn-of-plenty shaped backpack that appears to overflow with miniature harvest vegetables and autumn flowers.'),
  
  -- Beach Theme Items (furniture type, coins, tradeable)
  ('Tropical Horizon Walls', 'tropical-horizon-walls', 'furniture', 'coins', 800, 600, 1, 'Painted ocean vista with sailboats drifting across turquoise waters and seagulls soaring through cotton candy clouds.'),
  ('Sandy Shore Floor', 'sandy-shore-floor', 'furniture', 'coins', 800, 600, 1, 'Realistic beach sand texture complete with scattered seashells, starfish, and occasional foam from gentle waves.'),
  ('Driftwood Lounger', 'driftwood-lounger', 'furniture', 'coins', 1200, 900, 1, 'A rustic beach chair crafted from weathered driftwood with coral-colored cushions and rope accents.'),
  ('Tiki Bar Counter', 'tiki-bar-counter', 'furniture', 'coins', 1500, 1200, 1, 'Bamboo and thatch counter decorated with carved tikis, tropical flowers, and colorful paper umbrellas.'),
  ('Coconut Cooler Fridge', 'coconut-cooler-fridge', 'furniture', 'coins', 1800, 1400, 1, 'A refrigerator designed to look like a giant coconut, complete with palm frond details and a grass skirt base.'),
  
  -- Farmhouse Theme Items
  ('Barn Board Walls', 'barn-board-walls', 'furniture', 'coins', 800, 600, 1, 'Authentic red barn siding with visible wood grain, occasional knotholes, and vintage farm equipment mounted as decoration.'),
  ('Checkerboard Kitchen Tiles', 'checkerboard-kitchen-tiles', 'furniture', 'coins', 800, 600, 1, 'Classic black and white ceramic tiles with a slightly worn, homey appearance and subtle scuff marks.'),
  ('Hay Bale Bench', 'hay-bale-bench', 'furniture', 'coins', 1000, 750, 1, 'A rectangular hay bale wrapped with plaid ribbon, topped with a gingham cushion for comfort.'),
  ('Cast Iron Stove', 'cast-iron-stove', 'furniture', 'coins', 2000, 1600, 1, 'A vintage black wood-burning stove with copper kettles on top and decorative scrollwork on the doors.'),
  ('Mason Jar Cabinet', 'mason-jar-cabinet', 'furniture', 'coins', 1600, 1200, 1, 'Glass-front cupboard filled with preserved fruits, pickled vegetables, and homemade jams in labeled mason jars.'),
  
  -- Fruity Theme Items
  ('Fruit Salad Wallpaper', 'fruit-salad-wallpaper', 'furniture', 'coins', 800, 600, 1, 'Cheerful pattern of oversized watermelons, pineapples, cherries, and citrus slices on a mint green background.'),
  ('Berry Patch Carpet', 'berry-patch-carpet', 'furniture', 'coins', 800, 600, 1, 'Plush carpeting designed to look like a field of strawberries, blueberries, and raspberries growing on vines.'),
  ('Giant Orange Slice Chair', 'giant-orange-slice-chair', 'furniture', 'coins', 1400, 1100, 1, 'A semicircular seat shaped and colored like a fresh orange slice, complete with detailed pulp texture.'),
  ('Pineapple Prep Station', 'pineapple-prep-station', 'furniture', 'coins', 1700, 1300, 1, 'A kitchen island shaped like a halved pineapple with a cutting board top and tropical fruit storage below.'),
  ('Watermelon Refrigerator', 'watermelon-refrigerator', 'furniture', 'coins', 1900, 1500, 1, 'A round fridge designed as a giant watermelon with green striped exterior and pink interior shelving.'),
  
  -- Pumpkin Patch Theme Items
  ('Autumn Vineyard Walls', 'autumn-vineyard-walls', 'furniture', 'coins', 800, 600, 1, 'Climbing vines with orange and green pumpkins, golden leaves, and twisted branches against a sunset orange background.'),
  ('Fallen Leaves Floor', 'fallen-leaves-floor', 'furniture', 'coins', 800, 600, 1, 'A carpet of autumn leaves in reds, oranges, and yellows with occasional acorns scattered throughout.'),
  ('Pumpkin Carriage Seat', 'pumpkin-carriage-seat', 'furniture', 'coins', 1600, 1200, 1, 'An ornate chair carved from a giant pumpkin with gold vine details and burgundy velvet cushions.'),
  ('Harvest Hutch', 'harvest-hutch', 'furniture', 'coins', 1800, 1400, 1, 'A tall wooden cabinet with chicken wire doors, displaying miniature gourds, corn husks, and autumn preserves.'),
  ('Scarecrow Spice Rack', 'scarecrow-spice-rack', 'furniture', 'coins', 1300, 1000, 1, 'A friendly scarecrow figure whose outstretched arms and pockets hold various spice jars and seasonings.'),
  
  -- Sleepover Theme Items
  ('Glow Galaxy Walls', 'glow-galaxy-walls', 'furniture', 'coins', 900, 700, 1, 'Deep purple walls decorated with luminescent stars, planets, and shooting stars in soft pastel colors.'),
  ('Patchwork Quilt Floor', 'patchwork-quilt-floor', 'furniture', 'coins', 900, 700, 1, 'Soft flooring designed like a giant handmade quilt with different patterns and colors in each square.'),
  ('Sleeping Bag Sofa', 'sleeping-bag-sofa', 'furniture', 'coins', 1300, 1000, 1, 'A couch made from rolled-up sleeping bags in rainbow colors, tied together with friendship bracelet cords.'),
  ('Midnight Snack Station', 'midnight-snack-station', 'furniture', 'coins', 1500, 1200, 1, 'A wheeled cart loaded with popcorn machines, candy jars, and compartments for chips and treats.'),
  ('Hot Chocolate Bar', 'hot-chocolate-bar', 'furniture', 'coins', 1400, 1100, 1, 'A cozy wooden stand with copper mugs, marshmallow dispensers, whipped cream canisters, and various cocoa flavors.');

-- Insert products using the temp table and lookup joins
INSERT INTO dbo.product (name, slug, productTypeId, priceTypeId, price, premiumPrice, isTradeable, isNew, isPromotional, isExclusive, description, sku)
SELECT 
  pd.name,
  pd.slug,
  pt.productTypeId,
  prt.priceTypeId,
  pd.price,
  pd.premiumPrice,
  pd.isTradeable,
  1, -- isNew
  0, -- isPromotional
  0, -- isExclusive
  pd.description,
  UPPER(LEFT(pd.slug, 3)) + '-' + FORMAT(ROW_NUMBER() OVER (ORDER BY pd.slug), '00000') -- Generate SKU with sequential numbering
FROM @ProductData pd
JOIN dbo.productType pt ON pt.typeCode = pd.productType
JOIN dbo.priceType prt ON prt.priceTypeCode = pd.priceType;
GO

-- Product to Subcategory mappings using VALUES constructor
INSERT INTO dbo.productSubcategory (productId, subcategoryId)
SELECT p.productId, s.subcategoryId
FROM (VALUES
  -- Virtual Pets mappings
  ('cocoa-corgi', 'regular-pets'),
  ('stardust-dragon', 'mythical-pets'),
  ('pocket-hedgehog', 'little-pets'),
  ('rainbow-octopus', 'aquatic-pets'),
  
  -- Pet Clothing mappings
  ('star-sweater', 'tops'),
  ('cargo-shorts', 'bottoms-skirts'),
  ('cloud-sneakers', 'shoes'),
  ('red-rocker-collar', 'accessories'),
  
  -- Fall Clothing mappings (each item mapped to both fall-clothes and its type)
  ('harvest-moon-hoodie', 'fall-clothes'),
  ('harvest-moon-hoodie', 'tops'),
  ('maple-leaf-sweater', 'fall-clothes'),
  ('maple-leaf-sweater', 'tops'),
  ('scarecrow-flannel', 'fall-clothes'),
  ('scarecrow-flannel', 'tops'),
  ('pumpkin-patch-overalls', 'fall-clothes'),
  ('pumpkin-patch-overalls', 'bottoms-skirts'),
  ('autumn-plaid-skirt', 'fall-clothes'),
  ('autumn-plaid-skirt', 'bottoms-skirts'),
  ('cider-mill-pants', 'fall-clothes'),
  ('cider-mill-pants', 'bottoms-skirts'),
  ('leaf-pile-boots', 'fall-clothes'),
  ('leaf-pile-boots', 'shoes'),
  ('acorn-cap-slippers', 'fall-clothes'),
  ('acorn-cap-slippers', 'shoes'),
  ('harvest-festival-scarf', 'fall-clothes'),
  ('harvest-festival-scarf', 'accessories'),
  ('turkey-trot-hat', 'fall-clothes'),
  ('turkey-trot-hat', 'accessories'),
  ('apple-cider-mittens', 'fall-clothes'),
  ('apple-cider-mittens', 'accessories'),
  ('cornucopia-backpack', 'fall-clothes'),
  ('cornucopia-backpack', 'accessories'),
  
  -- Beach Theme (both theme and item type)
  ('tropical-horizon-walls', 'beach-theme'),
  ('tropical-horizon-walls', 'wallpaper-flooring'),
  ('sandy-shore-floor', 'beach-theme'),
  ('sandy-shore-floor', 'wallpaper-flooring'),
  ('driftwood-lounger', 'beach-theme'),
  ('driftwood-lounger', 'seats'),
  ('tiki-bar-counter', 'beach-theme'),
  ('tiki-bar-counter', 'kitchen'),
  ('coconut-cooler-fridge', 'beach-theme'),
  ('coconut-cooler-fridge', 'kitchen'),
  
  -- Farmhouse Theme (both theme and item type)
  ('barn-board-walls', 'farmhouse-theme'),
  ('barn-board-walls', 'wallpaper-flooring'),
  ('checkerboard-kitchen-tiles', 'farmhouse-theme'),
  ('checkerboard-kitchen-tiles', 'wallpaper-flooring'),
  ('hay-bale-bench', 'farmhouse-theme'),
  ('hay-bale-bench', 'seats'),
  ('cast-iron-stove', 'farmhouse-theme'),
  ('cast-iron-stove', 'kitchen'),
  ('mason-jar-cabinet', 'farmhouse-theme'),
  ('mason-jar-cabinet', 'kitchen'),
  
  -- Fruity Theme (both theme and item type)
  ('fruit-salad-wallpaper', 'fruity-theme'),
  ('fruit-salad-wallpaper', 'wallpaper-flooring'),
  ('berry-patch-carpet', 'fruity-theme'),
  ('berry-patch-carpet', 'wallpaper-flooring'),
  ('giant-orange-slice-chair', 'fruity-theme'),
  ('giant-orange-slice-chair', 'seats'),
  ('pineapple-prep-station', 'fruity-theme'),
  ('pineapple-prep-station', 'kitchen'),
  ('watermelon-refrigerator', 'fruity-theme'),
  ('watermelon-refrigerator', 'kitchen'),
  
  -- Pumpkin Patch Theme (both theme and item type and fall room items)
  ('autumn-vineyard-walls', 'pumpkin-patch-theme'),
  ('autumn-vineyard-walls', 'wallpaper-flooring'),
  ('autumn-vineyard-walls', 'fall-room-items'),
  ('fallen-leaves-floor', 'pumpkin-patch-theme'),
  ('fallen-leaves-floor', 'wallpaper-flooring'),
  ('fallen-leaves-floor', 'fall-room-items'),
  ('pumpkin-carriage-seat', 'pumpkin-patch-theme'),
  ('pumpkin-carriage-seat', 'seats'),
  ('pumpkin-carriage-seat', 'fall-room-items'),
  ('harvest-hutch', 'pumpkin-patch-theme'),
  ('harvest-hutch', 'kitchen'),
  ('harvest-hutch', 'fall-room-items'),
  ('scarecrow-spice-rack', 'pumpkin-patch-theme'),
  ('scarecrow-spice-rack', 'kitchen'),
  ('scarecrow-spice-rack', 'fall-room-items'),
  
  -- Sleepover Theme (both theme and item type)
  ('glow-galaxy-walls', 'sleepover-theme'),
  ('glow-galaxy-walls', 'wallpaper-flooring'),
  ('patchwork-quilt-floor', 'sleepover-theme'),
  ('patchwork-quilt-floor', 'wallpaper-flooring'),
  ('sleeping-bag-sofa', 'sleepover-theme'),
  ('sleeping-bag-sofa', 'seats'),
  ('midnight-snack-station', 'sleepover-theme'),
  ('midnight-snack-station', 'kitchen'),
  ('hot-chocolate-bar', 'sleepover-theme'),
  ('hot-chocolate-bar', 'kitchen')
) AS m(productSlug, subcategorySlug)
JOIN dbo.product p ON p.slug = m.productSlug
JOIN dbo.subcategory s ON s.slug = m.subcategorySlug;
GO