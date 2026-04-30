
# 插入admin用户
mysql -u root -p123456 web_db -e "INSERT INTO Users (Username, Password, Remember) VALUES ('admin', '123456', FALSE);"

# 插入示例作品数据
mysql -u root -p123456 web_db -e "INSERT INTO Works (Title, Author, Category, Description, UploadDate, Status, FilePath) VALUES ('Vue.js前端框架设计', '张三', '前端开发', '展示如何使用Vue.js构建现代化的前端应用', '2026-03-08', '已发布', '');"
mysql -u root -p123456 web_db -e "INSERT INTO Works (Title, Author, Category, Description, UploadDate, Status, FilePath) VALUES ('机器学习算法研究', '李四', '人工智能', '研究几种常见的机器学习算法', '2026-03-07', '已发布', '');"
mysql -u root -p123456 web_db -e "INSERT INTO Works (Title, Author, Category, Description, UploadDate, Status, FilePath) VALUES ('数据库优化方案', '王五', '后端开发', '提出一套完整的数据库优化方案', '2026-03-06', '草稿', '');"
mysql -u root -p123456 web_db -e "INSERT INTO Works (Title, Author, Category, Description, UploadDate, Status, FilePath) VALUES ('移动应用UI设计', '赵六', 'UI设计', '展示如何设计现代化的移动应用UI', '2026-03-05', '已发布', '');"
mysql -u root -p123456 web_db -e "INSERT INTO Works (Title, Author, Category, Description, UploadDate, Status, FilePath) VALUES ('Python数据分析', '钱七', '人工智能', '介绍如何使用Python进行数据分析', '2026-03-04', '草稿', '');"
