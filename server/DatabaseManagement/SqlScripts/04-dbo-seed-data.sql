-- Get user created in previous step to be creator of all demo data
DECLARE @coinsPriceTypeId INT = 1;
DECLARE @usdPriceTypeId INT = 2;

INSERT INTO dbo.productType (typeName, typeCode, description) VALUES
  ('Pet', 'pet', 'Virtual pets for adoption'),
  ('Furniture', 'furniture', 'Room decorations and furniture'),
  ('Clothing', 'clothing', 'Outfits and accessories'),
  ('Food', 'food', 'Pet food and treats'),
  ('Bundle', 'bundle', 'Product bundles and packages');

INSERT INTO dbo.category (name, slug, displayOrder) 
SELECT name, slug, displayOrder
FROM (VALUES 
  ('Virtual Pets', 'virtual-pets', 1),
  ('Seasonal', 'seasonal', 2),
  ('Clothing', 'clothing', 3),
  ('Room Themes', 'room-themes', 4),
  ('Room Items', 'room-items', 5)
) AS v(name, slug, displayOrder);

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
  ('Seasonal', 'Fall Clothes', 'fall-clothes', 3),
  -- Clothing subcategory
  ('Clothing', 'Tops', 'tops', 1),
  ('Clothing', 'Bottoms & Skirts', 'bottoms-skirts', 2),
  ('Clothing', 'Dresses & Jumpers', 'dresses-jumpers', 2),
  ('Clothing', 'Shoes', 'shoes', 4),
  ('Clothing', 'Accessories', 'accessories', 5),
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
  ('Room Items', 'Beds', 'beds', 4),
  ('Room Items', 'Carpets & Rugs', 'carpets-rugs', 5)
) AS s(categoryName, name, slug, displayOrder)
JOIN dbo.category c ON c.name = s.categoryName;


-- Create temporary table for products to simplify insertion
DECLARE @ProductData TABLE (
  name NVARCHAR(200),
  slug NVARCHAR(200),
  productType NVARCHAR(20),
  priceTypeId INT,
  price DECIMAL(10,2),
  premiumPrice DECIMAL(10,2),
  isTradeable BIT,
  description NVARCHAR(MAX)
);

-- Insert all product data
INSERT INTO @ProductData VALUES
  -- Virtual Pets
  ('Cocoa Corgi', 'cocoa-corgi', 'pet', @coinsPriceTypeId, 10000, 8000, 0, 'This adorable short-legged pup loves to waddle around your room and perform silly tricks! With fluffy cocoa-colored fur and a perpetual smile, this pup brings joy to every virtual playdate.'),
  ('Orange Corgi', 'orange-corgi', 'pet', @coinsPriceTypeId, 10000, 8000, 0, 'This adorable short-legged pup loves to waddle around your room and perform silly tricks! With fluffy orange fur and a perpetual smile, this pup brings joy to every virtual playdate.'),
  ('Stardust Dragon', 'stardust-dragon', 'pet', @coinsPriceTypeId, 12000, 10000, 0, 'A shimmering purple dragon with constellation patterns on its wings. This magical companion breathes sparkles instead of fire.'),
  ('Pocket Hedgehog', 'pocket-hedgehog', 'pet', @usdPriceTypeId, 10.99, 8.99, 0, 'Tiny but mighty! This palm-sized hedgehog makes soft purring sounds when happy.'),
  ('Rainbow Octopus', 'rainbow-octopus', 'pet', @coinsPriceTypeId, 10000, 8000, 0, 'This color-changing octopus creates beautiful ink art in its underwater room! Watch as it uses all eight tentacles to juggle shells and play underwater instruments.'),
  ('Crystal Fox', 'crystal-fox', 'pet', @coinsPriceTypeId, 9000, 7000, 0, 'This mystical fox has translucent blue fur that sparkles! It loves exploring the gem mines and playing hide-and-seek among shimmering rocks.'),
  ('Cloud Sheep', 'cloud-sheep', 'pet', @usdPriceTypeId, 10.99, 8.99, 0, 'Fluffy as a summer cloud, this dreamy sheep loves to graze in meadows and take peaceful naps!'),
  ('Snowflake Penguin', 'snowflake-penguin', 'pet', @coinsPriceTypeId, 10000, 8000, 0, 'This arctic cutie loves to slide on its belly across icy surfaces and play with snowballs in its the winter wonderland!'),
  ('Sunset Tiger', 'sunset-tiger', 'pet', @usdPriceTypeId, 10.99, 8.99, 0, 'With stripes in beautiful sunset colors, this tiger brings warmth wherever it roams! It loves to stretch dramatically and practice pouncing on toy mice. This playful feline enjoys basking in sunny spots.'),
  ('Mini Mint Turtle', 'mini-mint-turtle', 'pet', @coinsPriceTypeId, 10000, 8000, 0, 'This mini zen companion moves at a peaceful pace and loves tending to underwater gardens! Perfect for players who enjoy a calm, relaxing pet experience.'),
  ('Cherry Blossom Panda', 'cherry-blossom-panda', 'pet', @coinsPriceTypeId, 12000, 10000, 0, 'Adorned with delicate pink flower markings, this gentle panda practices tai chi movements! It loves munching bamboo shoots and rolling playfully on soft grass.'),
  ('Red Velvet Cat', 'red-velvet-cat', 'pet', @coinsPriceTypeId, 9000, 7000, 0, 'This cuddly cat looks like its favorite dish - a red velvet cake slice with cream cheese icing! You may find it baking in the kitchen in its free time.'),

  -- Clothing
  ('Star Sweater', 'star-sweater', 'clothing', @coinsPriceTypeId, 2000, 1500, 0, 'A cozy knit sweater with multi-colored stars! Perfect for an evening stroll in the park.'),
  ('Pink Blouse', 'pink-blouse', 'clothing', @coinsPriceTypeId, 1500, 1000, 0, 'A casual pink blouse for every day wear!'),
  ('Galaxy Tank', 'galaxy-tank', 'clothing', @coinsPriceTypeId, 2000, 1500, 0, 'A sleeveless tank top with a deep space design featuring swirling galaxies, distant stars, and nebula clouds in deep purples, blues, and silver. Perfect for aspiring astronauts and space explorers.'),
  ('Ocean Wave Top', 'ocean-wave-top', 'clothing', @coinsPriceTypeId, 2000, 1500, 0, 'A light blue t-shirt with flowing wave patterns that seem to ripple across the fabric. The waves are rendered in different shades of blue and white, creating a calming oceanic feel.'),
  ('Cargo Shorts', 'cargo-shorts', 'clothing', @coinsPriceTypeId, 2000, 1500, 0, 'Equipped with pockets big enough to hold all your favorite treats! These khaki shorts are perfect for pets who love exploring and collecting treasures.'),
  ('Rainbow Tutu', 'rainbow-tutu', 'clothing', @coinsPriceTypeId, 1500, 1000, 0, 'A bouncy tulle skirt with layers in all the colors of the rainbow. Each layer is a different color, creating a magical rainbow effect when your pet twirls around.'),
  ('Starlight Dress', 'starlight-dress', 'clothing', @coinsPriceTypeId, 2000, 1500, 0, 'A flowing purple dress with tiny silver stars scattered across the fabric and a shimmery silver sash around the waist. Perfect for evening adventures and stargazing with friends.'),
  ('Sunflower Sundress', 'sunflower-sundress', 'clothing', @coinsPriceTypeId, 2000, 1500, 0, 'A cheerful yellow sundress with large sunflower prints and thin shoulder straps. The hem features a border of green leaves, bringing sunshine to any virtual wardrobe.'),
  ('Cloud Sneakers', 'cloud-sneakers', 'clothing', @coinsPriceTypeId, 1500, 1000, 0, 'These puffy white sneakers let your pet bounce extra high and leave little cloud puffs with each step. Great for athletic activities!'),
  ('Red Rocker Collar', 'red-rocker-collar', 'clothing', @coinsPriceTypeId, 1500, 1000, 0, 'This red collar shows your pet is really into rock and roll!'),
  
  -- Fall Clothing Items
  ('Harvest Moon Hoodie', 'harvest-moon-hoodie', 'clothing', @coinsPriceTypeId, 2500, 2000, 0, 'A cozy burnt orange hoodie featuring a glowing moon design with falling autumn leaves. The front pocket is shaped like a basket for collecting acorns.'),
  ('Maple Leaf Sweater', 'maple-leaf-sweater', 'clothing', @coinsPriceTypeId, 2200, 1800, 0, 'A cozy autumn sweater featuring beautiful maple leaf patterns in warm fall colors. The leaves are scattered across the fabric in shades of red, orange, and golden yellow, perfect for celebrating the changing seasons.'),
  ('Pumpkin Patch Overalls', 'pumpkin-patch-overalls', 'clothing', @coinsPriceTypeId, 2300, 1900, 0, 'Denim overalls with orange pumpkin patches on the knees and an embroidered vine pattern climbing up one leg.'),
  ('Autumn Plaid Skirt', 'autumn-plaid-skirt', 'clothing', @coinsPriceTypeId, 1800, 1400, 0, 'A twirly skirt in warm browns, oranges, and yellows with a leaf-print hem that rustles when your pet walks.'),
  ('Cider Mill Pants', 'cider-mill-pants', 'clothing', @coinsPriceTypeId, 2000, 1600, 0, 'Corduroy pants in deep cinnamon brown with cuffs that can be rolled up to reveal a gingham pattern with apple embroidery.'),
  ('Leaf Pile Boots', 'leaf-pile-boots', 'clothing', @coinsPriceTypeId, 1700, 1300, 0, 'Tall rain boots that look like your pet is walking through crunchy autumn leaves. Each step shows colorful leaves stuck to the soles.'),
  ('Acorn Cap Slippers', 'acorn-cap-slippers', 'clothing', @coinsPriceTypeId, 1200, 900, 0, 'Cozy brown slippers shaped like acorn caps with fuzzy wool lining, perfect for indoor autumn days.'),
  ('Harvest Festival Scarf', 'harvest-festival-scarf', 'clothing', @coinsPriceTypeId, 1000, 750, 0, 'A long knitted scarf that gradients from green to yellow to orange to red, mimicking the changing fall foliage.'),
  ('Turkey Trot Hat', 'turkey-trot-hat', 'clothing', @coinsPriceTypeId, 1500, 1200, 0, 'A pilgrim-style hat with a turkey feather tucked in the band that bobbles when your pet moves.'),
  ('Apple Cider Mittens', 'apple-cider-mittens', 'clothing', @coinsPriceTypeId, 800, 600, 0, 'Soft red mittens woven with quality yarn, complete with green stems on top to look like apples.'),
  
  -- Beach Theme Items
  ('Tropical Horizon Wallpaper', 'tropical-horizon-wallpaper', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Painted ocean vista with sailboats drifting across turquoise waters and seagulls soaring through cotton candy clouds.'),
  ('Sandy Shore Rug', 'sandy-shore-rug', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Realistic beach sand texture complete with scattered seashells, starfish, and occasional foam from gentle waves.'),
  ('Driftwood Lounger', 'driftwood-lounger', 'furniture', @coinsPriceTypeId, 1200, 900, 1, 'A rustic beach chair crafted from weathered driftwood with coral-colored cushions and rope accents.'),
  ('Tiki Bar Counter', 'tiki-bar-counter', 'furniture', @coinsPriceTypeId, 1500, 1200, 1, 'Bamboo and thatch counter decorated with carved tikis, tropical flowers, and colorful paper umbrellas.'),
  ('Coconut Cooler Fridge', 'coconut-cooler-fridge', 'furniture', @coinsPriceTypeId, 1800, 1400, 1, 'A refrigerator designed to look like a giant coconut, complete with palm frond details and a grass skirt base.'),
  ('Driftwood Canopy Bed', 'driftwood-canopy-bed', 'furniture', @coinsPriceTypeId, 1800, 1400, 1, 'A four-poster bed made from weathered driftwood with flowing white curtains and seashell decorations.'),

  -- Farmhouse Theme Items
  ('Barn Board Wallpaper', 'barn-board-wallpaper', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Authentic red barn siding with visible wood grain, occasional knotholes, and vintage farm equipment mounted as decoration.'),
  ('Checkerboard Kitchen Tiles', 'checkerboard-kitchen-tiles', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Classic black and white ceramic tiles with a slightly worn, homey appearance and subtle scuff marks.'),
  ('Hay Bale Bench', 'hay-bale-bench', 'furniture', @coinsPriceTypeId, 1000, 750, 1, 'A rectangular hay bale wrapped with plaid ribbon, topped with a gingham cushion for comfort.'),
  ('Cast Iron Stove', 'cast-iron-stove', 'furniture', @coinsPriceTypeId, 2000, 1600, 1, 'A vintage black wood-burning stove with copper kettles on top and decorative scrollwork on the doors.'),
  ('Mason Jar Cabinet', 'mason-jar-cabinet', 'furniture', @coinsPriceTypeId, 1600, 1200, 1, 'Glass-front cupboard filled with preserved fruits, pickled vegetables, and homemade jams in labeled mason jars.'),
  ('Quilted Barn Bed', 'quilted-barn-bed', 'furniture', @coinsPriceTypeId, 2000, 1600, 1, 'A rustic wooden bed frame with a headboard designed like a red barn door and a handmade patchwork quilt.'),

  -- Fruity Theme Items
  ('Fruit Salad Wallpaper', 'fruit-salad-wallpaper', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Cheerful pattern of oversized watermelons, pineapples, cherries, and citrus slices on a mint green background.'),
  ('Berry Patch Carpet', 'berry-patch-carpet', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Plush carpeting designed to look like a field of strawberries, blueberries, and raspberries growing on vines.'),
  ('Giant Orange Slice Chair', 'giant-orange-slice-chair', 'furniture', @coinsPriceTypeId, 1400, 1100, 1, 'A semicircular seat shaped and colored like a fresh orange slice, complete with detailed pulp texture.'),
  ('Pineapple Prep Station', 'pineapple-prep-station', 'furniture', @coinsPriceTypeId, 1700, 1300, 1, 'A kitchen island shaped like a halved pineapple with a cutting board top and tropical fruit storage below.'),
  ('Watermelon Refrigerator', 'watermelon-refrigerator', 'furniture', @coinsPriceTypeId, 1900, 1500, 1, 'A round fridge designed as a giant watermelon with green striped exterior and pink interior shelving.'),
  ('Blueberry Pie Bed', 'blueberry-pie-bed', 'furniture', @coinsPriceTypeId, 2000, 1800, 1, 'A bed shaped like a freshly baked blueberry pie with golden flaky crust, gooey purple filling oozing between lattice strips.'),

  -- Pumpkin Patch Theme Items
  ('Autumn Vineyard Wallpaper', 'autumn-vineyard-wallpaper', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Climbing vines with orange and green pumpkins, golden leaves, and twisted branches against a sunset orange background.'),
  ('Corn Maze Flooring', 'corn-maze-flooring', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'Flooring designed like a miniature corn maze with golden pathways winding between rows of tiny corn stalks.'),
  ('Fallen Leaves Carpet', 'fallen-leaves-carpet', 'furniture', @coinsPriceTypeId, 800, 600, 1, 'A carpet of autumn leaves in reds, oranges, and yellows with occasional acorns scattered throughout.'),
  ('Pumpkin Carriage Seat', 'pumpkin-carriage-seat', 'furniture', @coinsPriceTypeId, 1600, 1200, 1, 'An ornate chair carved from a giant pumpkin with gold vine details and burgundy velvet cushions.'),
  ('Harvest Hutch', 'harvest-hutch', 'furniture', @coinsPriceTypeId, 1800, 1400, 1, 'A tall wooden cabinet with chicken wire doors, displaying miniature gourds, corn husks, and autumn preserves.'),
  ('Harvest Moon Bed', 'harvest-moon-bed', 'furniture', @coinsPriceTypeId, 1600, 1200, 1, 'A bed with an ornate headboard carved to look like autumn vines and a warm orange comforter with gold leaf details.'),

  -- Sleepover Theme Items
  ('Glow Galaxy Wallpaper', 'glow-galaxy-wallpaper', 'furniture', @coinsPriceTypeId, 900, 700, 1, 'Deep purple walls decorated with luminescent stars, planets, and shooting stars in soft pastel colors.'),
  ('Patchwork Flooring', 'patchwork-flooring', 'furniture', @coinsPriceTypeId, 900, 700, 1, 'Soft flooring designed like a giant handmade quilt with different patterns and colors in each square.'),
  ('Sleeping Bag Sofa', 'sleeping-bag-sofa', 'furniture', @coinsPriceTypeId, 1300, 1000, 1, 'A couch made from rolled-up sleeping bags in rainbow colors, tied together with friendship bracelet cords.'),
  ('Midnight Snack Station', 'midnight-snack-station', 'furniture', @coinsPriceTypeId, 1500, 1200, 1, 'A wheeled cart loaded with popcorn machines, candy jars, and compartments for chips and treats.'),
  ('Hot Chocolate Bar', 'hot-chocolate-bar', 'furniture', @coinsPriceTypeId, 1400, 1100, 1, 'A cozy wooden stand with copper mugs, marshmallow dispensers, whipped cream canisters, and various cocoa flavors.'),
  ('Bunk Bed Fort', 'bunk-bed-fort', 'furniture', @coinsPriceTypeId, 1500, 1200, 1, 'A fun bunk bed with built-in ladder, string lights, and colorful blankets draped to create cozy fort spaces.');

-- Insert products using the temp table and lookup joins
INSERT INTO dbo.product (name, slug, productTypeId, priceTypeId, price, premiumPrice, isTradeable, isNew, isPromotional, isExclusive, isDemoProduct, description, sku)
SELECT 
  pd.name,
  pd.slug,
  pt.productTypeId,
  pd.priceTypeId,
  pd.price,
  pd.premiumPrice,
  pd.isTradeable,
  1 AS isNew,
  0 AS isPromotional,
  0 AS isExclusive,
  1 AS isDemoProduct,
  pd.description,
  UPPER(LEFT(pd.slug, 3)) + '-' + FORMAT(ROW_NUMBER() OVER (ORDER BY pd.slug), '00000') -- Generate SKU with sequential numbering
FROM @ProductData pd
JOIN dbo.productType pt ON pt.typeCode = pd.productType


INSERT INTO dbo.productSubcategory (productId, subcategoryId)
SELECT p.productId, s.subcategoryId
FROM (VALUES
  -- Virtual Pets mappings
  ('cocoa-corgi', 'regular-pets'),
  ('stardust-dragon', 'mythical-pets'),
  ('pocket-hedgehog', 'little-pets'),
  ('rainbow-octopus', 'aquatic-pets'),
  ('crystal-fox', 'mythical-pets'),
  ('cloud-sheep', 'regular-pets'),
  ('snowflake-penguin', 'mythical-pets'),
  ('sunset-tiger', 'mythical-pets'),
  ('mini-mint-turtle', 'little-pets'),
  ('cherry-blossom-panda', 'mythical-pets'),
  ('red-velvet-cat', 'regular-pets'),
  ('orange-corgi', 'regular-pets'),
  
  -- Clothing mappings
  ('star-sweater', 'tops'),
  ('pink-blouse', 'tops'),
  ('galaxy-tank', 'tops'),
  ('ocean-wave-top', 'tops'),
  ('cargo-shorts', 'bottoms-skirts'),
  ('rainbow-tutu', 'bottoms-skirts'),
  ('starlight-dress', 'dresses-jumpers'),
  ('sunflower-sundress', 'dresses-jumpers'),
  ('cloud-sneakers', 'shoes'),
  ('red-rocker-collar', 'accessories'),
  
  -- Fall Clothing mappings (each item mapped to both fall-clothes and its type)
  ('harvest-moon-hoodie', 'fall-clothes'),
  ('harvest-moon-hoodie', 'tops'),
  ('maple-leaf-sweater', 'fall-clothes'),
  ('maple-leaf-sweater', 'tops'),
  ('pumpkin-patch-overalls', 'fall-clothes'),
  ('pumpkin-patch-overalls', 'dresses-jumpers'),
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
  ('tropical-horizon-wallpaper', 'beach-theme'),
  ('tropical-horizon-wallpaper', 'wallpaper-flooring'),
  ('sandy-shore-rug', 'beach-theme'),
  ('sandy-shore-rug', 'carpets-rugs'),
  ('driftwood-lounger', 'beach-theme'),
  ('driftwood-lounger', 'seats'),
  ('tiki-bar-counter', 'beach-theme'),
  ('tiki-bar-counter', 'kitchen'),
  ('coconut-cooler-fridge', 'beach-theme'),
  ('coconut-cooler-fridge', 'kitchen'),
  ('driftwood-canopy-bed', 'beach-theme'),
  ('driftwood-canopy-bed', 'beds'),
  
  -- Farmhouse Theme (both theme and item type)
  ('barn-board-wallpaper', 'farmhouse-theme'),
  ('barn-board-wallpaper', 'wallpaper-flooring'),
  ('checkerboard-kitchen-tiles', 'farmhouse-theme'),
  ('checkerboard-kitchen-tiles', 'wallpaper-flooring'),
  ('hay-bale-bench', 'farmhouse-theme'),
  ('hay-bale-bench', 'seats'),
  ('cast-iron-stove', 'farmhouse-theme'),
  ('cast-iron-stove', 'kitchen'),
  ('mason-jar-cabinet', 'farmhouse-theme'),
  ('mason-jar-cabinet', 'kitchen'),
  ('quilted-barn-bed', 'farmhouse-theme'),
  ('quilted-barn-bed', 'beds'),
  
  -- Fruity Theme (both theme and item type)
  ('fruit-salad-wallpaper', 'fruity-theme'),
  ('fruit-salad-wallpaper', 'wallpaper-flooring'),
  ('berry-patch-carpet', 'fruity-theme'),
  ('berry-patch-carpet', 'carpets-rugs'),
  ('giant-orange-slice-chair', 'fruity-theme'),
  ('giant-orange-slice-chair', 'seats'),
  ('pineapple-prep-station', 'fruity-theme'),
  ('pineapple-prep-station', 'kitchen'),
  ('watermelon-refrigerator', 'fruity-theme'),
  ('watermelon-refrigerator', 'kitchen'),
  ('blueberry-pie-bed', 'fruity-theme'),
  ('blueberry-pie-bed', 'beds'),
  
  -- Pumpkin Patch Theme (both theme and item type and fall room items)
  ('autumn-vineyard-wallpaper', 'pumpkin-patch-theme'),
  ('autumn-vineyard-wallpaper', 'wallpaper-flooring'),
  ('autumn-vineyard-wallpaper', 'fall-room-items'),
  ('corn-maze-flooring', 'pumpkin-patch-theme'),
  ('corn-maze-flooring', 'wallpaper-flooring'),
  ('corn-maze-flooring', 'fall-room-items'),
  ('fallen-leaves-carpet', 'pumpkin-patch-theme'),
  ('fallen-leaves-carpet', 'carpets-rugs'),
  ('fallen-leaves-carpet', 'fall-room-items'),
  ('pumpkin-carriage-seat', 'pumpkin-patch-theme'),
  ('pumpkin-carriage-seat', 'seats'),
  ('pumpkin-carriage-seat', 'fall-room-items'),
  ('harvest-hutch', 'pumpkin-patch-theme'),
  ('harvest-hutch', 'kitchen'),
  ('harvest-hutch', 'fall-room-items'),
  ('harvest-moon-bed', 'pumpkin-patch-theme'),
  ('harvest-moon-bed', 'beds'),
  ('harvest-moon-bed', 'fall-room-items'),
  
  -- Sleepover Theme (both theme and item type)
  ('glow-galaxy-wallpaper', 'sleepover-theme'),
  ('glow-galaxy-wallpaper', 'wallpaper-flooring'),
  ('patchwork-flooring', 'sleepover-theme'),
  ('patchwork-flooring', 'wallpaper-flooring'),
  ('sleeping-bag-sofa', 'sleepover-theme'),
  ('sleeping-bag-sofa', 'seats'),
  ('midnight-snack-station', 'sleepover-theme'),
  ('midnight-snack-station', 'kitchen'),
  ('hot-chocolate-bar', 'sleepover-theme'),
  ('hot-chocolate-bar', 'kitchen'),
  ('bunk-bed-fort', 'sleepover-theme'),
  ('bunk-bed-fort', 'beds')

) AS m(productSlug, subcategorySlug)
JOIN dbo.product p ON p.slug = m.productSlug
JOIN dbo.subcategory s ON s.slug = m.subcategorySlug;
GO