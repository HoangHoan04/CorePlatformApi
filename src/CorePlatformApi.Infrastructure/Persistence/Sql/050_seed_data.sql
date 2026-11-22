-- Sinh tự động từ docs/base_erp_core.sql — các phần EF Core không biểu diễn được.
INSERT INTO mdm.currencies (code, numeric_code, name, symbol, decimal_places, rounding_unit, symbol_position, sort_order) VALUES
 ('VND','704','Đồng Việt Nam','₫',0,1,'SUFFIX',1),
 ('USD','840','Đô la Mỹ','$',2,0.01,'PREFIX',2),
 ('EUR','978','Euro','€',2,0.01,'PREFIX',3),
 ('JPY','392','Yên Nhật','¥',0,1,'PREFIX',4),
 ('CNY','156','Nhân dân tệ','¥',2,0.01,'PREFIX',5),
 ('KRW','410','Won Hàn Quốc','₩',0,1,'PREFIX',6);

INSERT INTO mdm.countries (code, alpha3_code, numeric_code, name, name_en, phone_code, currency_code, flag_emoji, sort_order) VALUES
 ('VN','VNM','704','Việt Nam','Viet Nam','+84','VND','🇻🇳',1),
 ('US','USA','840','Hoa Kỳ','United States','+1','USD','🇺🇸',2),
 ('JP','JPN','392','Nhật Bản','Japan','+81','JPY','🇯🇵',3),
 ('KR','KOR','410','Hàn Quốc','Korea, Republic of','+82','KRW','🇰🇷',4),
 ('CN','CHN','156','Trung Quốc','China','+86','CNY','🇨🇳',5);

INSERT INTO config.languages (code, name, native_name, is_default, sort_order) VALUES
 ('vi-VN','Tiếng Việt','Tiếng Việt',true,1),
 ('en-US','Tiếng Anh','English',false,2);

INSERT INTO iam.modules (code, name, base_route, is_core, sort_order) VALUES
 ('CORE','Nền tảng lõi','/core',true,1),
 ('HRM','Quản trị nhân sự','/hrm',false,2),
 ('CRM','Quản lý khách hàng','/crm',false,3),
 ('WMS','Kho vận & vận hành','/wms',false,4),
 ('AI','Trí tuệ nhân tạo','/ai',false,5);

INSERT INTO iam.permission_actions (code, name, sort_order) VALUES
 ('VIEW','Xem',1),('CREATE','Thêm',2),('UPDATE','Sửa',3),('DELETE','Xóa',4),
 ('APPROVE','Duyệt',5),('EXPORT','Xuất file',6),('IMPORT','Nhập file',7),('PRINT','In',8),('MANAGE','Quản trị',9);

INSERT INTO iam.security_policies (tenant_id) VALUES (NULL);

INSERT INTO mdm.tax_types (code, name) VALUES
 ('VAT','Thuế giá trị gia tăng'),('CIT','Thuế thu nhập doanh nghiệp'),('PIT','Thuế thu nhập cá nhân'),
 ('IMPORT','Thuế nhập khẩu'),('SCT','Thuế tiêu thụ đặc biệt');

INSERT INTO mdm.tax_rates (tax_type_code, code, name, rate, calc_method, einvoice_code, effective_from, sort_order, is_default) VALUES
 ('VAT','VAT10','Thuế GTGT 10%',10,'PERCENT','10%','2009-01-01',1,true),
 ('VAT','VAT8','Thuế GTGT 8% (giảm theo nghị định)',8,'PERCENT','8%','2022-02-01',2,false),
 ('VAT','VAT5','Thuế GTGT 5%',5,'PERCENT','5%','2009-01-01',3,false),
 ('VAT','VAT0','Thuế GTGT 0%',0,'PERCENT','0%','2009-01-01',4,false),
 ('VAT','KCT','Không chịu thuế GTGT',NULL,'NOT_SUBJECT','KCT','2009-01-01',5,false),
 ('VAT','KKKNT','Không kê khai, tính nộp thuế GTGT',NULL,'NOT_DECLARED','KKKNT','2009-01-01',6,false);

INSERT INTO mdm.uom_categories (code, name) VALUES
 ('COUNT','Số lượng'),('WEIGHT','Khối lượng'),('VOLUME','Thể tích'),('LENGTH','Chiều dài'),('AREA','Diện tích'),('TIME','Thời gian');

INSERT INTO mdm.uoms (category_id, code, name, symbol, is_base, ratio_to_base, decimal_places, sort_order)
SELECT c.id, v.code, v.name, v.symbol, v.is_base, v.ratio, v.dp, v.so
FROM (VALUES
 ('COUNT','PCS','Cái','cái',true,1,0,1),('COUNT','BOX','Hộp','hộp',false,1,0,2),('COUNT','CTN','Thùng','thùng',false,1,0,3),
 ('COUNT','PACK','Gói','gói',false,1,0,4),('COUNT','SET','Bộ','bộ',false,1,0,5),
 ('WEIGHT','G','Gam','g',false,0.001,2,10),('WEIGHT','KG','Kilogam','kg',true,1,3,11),('WEIGHT','TON','Tấn','tấn',false,1000,3,12),
 ('VOLUME','ML','Mililít','ml',false,0.001,0,20),('VOLUME','L','Lít','l',true,1,3,21),('VOLUME','M3','Mét khối','m³',false,1000,3,22),
 ('LENGTH','CM','Xentimét','cm',false,0.01,2,30),('LENGTH','M','Mét','m',true,1,3,31),
 ('AREA','M2','Mét vuông','m²',true,1,3,40),
 ('TIME','HOUR','Giờ','giờ',true,1,2,50),('TIME','DAY','Ngày','ngày',false,8,2,51)
) AS v(cat, code, name, symbol, is_base, ratio, dp, so)
JOIN mdm.uom_categories c ON c.code = v.cat;

INSERT INTO ai.providers (code, name, api_style, base_url, is_self_hosted) VALUES
 ('OPENAI','OpenAI','OPENAI','https://api.openai.com/v1',false),
 ('ANTHROPIC','Anthropic','ANTHROPIC','https://api.anthropic.com',false),
 ('GEMINI','Google Gemini','GEMINI','https://generativelanguage.googleapis.com',false),
 ('OLLAMA','Model nội bộ (Ollama)','OPENAI','http://ollama:11434/v1',true);

INSERT INTO gateway.error_codes (code, http_status, category, default_message, is_retryable) VALUES
 ('VALIDATION_FAILED',400,'VALIDATION','Dữ liệu không hợp lệ',false),
 ('AUTH_UNAUTHORIZED',401,'AUTHENTICATION','Bạn chưa đăng nhập hoặc phiên đã hết hạn',false),
 ('AUTH_TOKEN_EXPIRED',401,'AUTHENTICATION','Phiên đăng nhập đã hết hạn',false),
 ('AUTH_TOKEN_REVOKED',401,'AUTHENTICATION','Phiên đăng nhập đã bị thu hồi',false),
 ('AUTH_ACCOUNT_LOCKED',403,'AUTHENTICATION','Tài khoản đã bị khóa',false),
 ('AUTH_PASSWORD_CHANGE_REQUIRED',403,'AUTHENTICATION','Bạn cần đổi mật khẩu trước khi tiếp tục',false),
 ('FORBIDDEN',403,'AUTHORIZATION','Bạn không có quyền thực hiện thao tác này',false),
 ('MODULE_NOT_ENABLED',403,'AUTHORIZATION','Phân hệ chưa được kích hoạt cho đơn vị của bạn',false),
 ('NOT_FOUND',404,'NOT_FOUND','Không tìm thấy dữ liệu',false),
 ('CONFLICT',409,'CONFLICT','Dữ liệu đã bị thay đổi bởi người khác, vui lòng tải lại',false),
 ('DUPLICATE',409,'CONFLICT','Dữ liệu đã tồn tại',false),
 ('RATE_LIMITED',429,'RATE_LIMIT','Bạn thao tác quá nhanh, vui lòng thử lại sau',true),
 ('INTERNAL_ERROR',500,'SYSTEM','Hệ thống gặp lỗi, vui lòng thử lại sau',true),
 ('SERVICE_UNAVAILABLE',503,'SYSTEM','Dịch vụ đang bảo trì hoặc tạm thời không khả dụng',true),
 ('UPSTREAM_TIMEOUT',504,'INTEGRATION','Dịch vụ phản hồi quá lâu',true);

INSERT INTO gateway.rate_limit_policies (code, name, partition_by, algorithm, permit_limit, window_seconds, segments) VALUES
 ('anonymous-ip','Request chưa đăng nhập theo IP','IP','SLIDING_WINDOW',60,60,6),
 ('login-strict','Chống dò mật khẩu tại /auth/login','IP','FIXED_WINDOW',10,60,NULL),
 ('default-user','Mặc định theo user','USER','SLIDING_WINDOW',300,60,6),
 ('default-tenant','Tổng theo tenant','TENANT','SLIDING_WINDOW',5000,60,6),
 ('partner-basic','Đối tác qua API key','API_KEY','TOKEN_BUCKET',120,60,NULL);
