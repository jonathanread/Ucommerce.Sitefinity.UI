INSERT INTO uCommerce_AdminPage SELECT 'searchsettings_aspx', ''
INSERT INTO uCommerce_AdminTab SELECT 'IndexFromScratch.ascx', (SELECT AdminPageId FROM ucommerce_adminpage WHERE FullName = 'searchsettings_aspx'), 1, 0, 'Common', 0, 0, 1