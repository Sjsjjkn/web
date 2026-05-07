@echo off
mysql -u root -p123456 web_db < init_database.sql
mysql -u root -p123456 web_db < seed_data.sql
echo Database initialized successfully
pause