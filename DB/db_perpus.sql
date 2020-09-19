
-- Setup  MySQL Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

CREATE SCHEMA IF NOT EXISTS `db_perpus` DEFAULT CHARACTER SET utf8 COLLATE utf8_bin;
USE `db_perpus`;

CREATE TABLE IF NOT EXISTS `db_perpus`.`user_category` (
	`uc_id` INT NOT NULL AUTO_INCREMENT,
	`uc_name` VARCHAR(45) NOT NULL,
	`uc_desc` VARCHAR(75) DEFAULT NULL,
	PRIMARY KEY (`uc_id`),
	UNIQUE KEY `uc_id_UNIQUE` (`uc_id`)
)
ENGINE = InnoDB;

LOCK TABLES `db_perpus`.`user_category` WRITE;
INSERT INTO `db_perpus`.`user_category` VALUES (1,'Site','Manajer Cabang'),(2,'Staff','Staff Perpustakaan'),(3,'Member','Member Perpustakaan');
UNLOCK TABLES;

CREATE TABLE IF NOT EXISTS `db_perpus`.`users` (
	`u_id` INT NOT NULL,
	`u_uc_id` INT NOT NULL,
	`u_username` VARCHAR(24) NOT NULL,
	`u_password` VARCHAR(16) NOT NULL,
	`u_rec_status` SMALLINT NOT NULL,
	`u_rec_createdby` VARCHAR(45) NOT NULL,
	`u_rec_created` DATETIME NOT NULL,
	`u_rec_updatedby` VARCHAR(45) DEFAULT NULL,
	`u_rec_updated` DATETIME NULL,
	`u_rec_deletedby` VARCHAR(45) DEFAULT NULL,
	`u_rec_deleted` DATETIME NULL,
	PRIMARY KEY (`u_id`,`u_uc_id`),
	UNIQUE KEY `u_id_UNIQUE` (`u_id`),
	CONSTRAINT `fk_userCategory_users` FOREIGN KEY (`u_uc_id`)
	REFERENCES `db_perpus`.`user_category` (`uc_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`user_photo` (
	`up_id` INT NOT NULL,
	`up_u_id` INT NOT NULL,
	`up_photo` BLOB NOT NULL,
	`up_filename` VARCHAR(45) NOT NULL,
	`up_rec_status` SMALLINT(6) NOT NULL,
	PRIMARY KEY (`up_id`, `up_u_id`),
	UNIQUE KEY `up_id_UNIQUE` (`up_id`),
	CONSTRAINT `fk_userPhoto_users` FOREIGN KEY (`up_u_id`)
	REFERENCES `db_perpus`.`users` (`u_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`site` (
	`s_id` INT NOT NULL,
	`s_u_id` INT NOT NULL,
	`s_fullname` VARCHAR(80) NOT NULL,
	`s_email` VARCHAR(45) NOT NULL,
	`s_contact` VARCHAR(20) NOT NULL,
	`s_address` VARCHAR(150) NOT NULL,
	`s_status` SMALLINT NOT NULL,
	`s_rec_status` SMALLINT NOT NULL,
	`s_rec_createdby` VARCHAR(45) NOT NULL,
	`s_rec_created` DATETIME NOT NULL,
	`s_rec_updatedby` VARCHAR(45) DEFAULT NULL,
	`s_rec_updated` DATETIME NULL,
	`s_rec_deletedby` VARCHAR(45) DEFAULT NULL,
	`s_rec_deleted` DATETIME NULL,
	PRIMARY KEY (`s_id`),
	UNIQUE KEY `s_id_UNIQUE` (`s_id`),
	CONSTRAINT `fk_users_site` FOREIGN KEY (`s_u_id`)
	REFERENCES `db_perpus`.`users` (`u_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`staff_category` (
	`sc_id` INT NOT NULL,
	`sc_name` VARCHAR(45) NOT NULL,
	`sc_desc` VARCHAR(75) DEFAULT NULL,
	PRIMARY KEY (`sc_id`),
	UNIQUE KEY `sc_id_UNIQUE` (`sc_id`)
)
ENGINE = InnoDB;

LOCK TABLES `db_perpus`.`staff_category` WRITE;
INSERT INTO `db_perpus`.`staff_category` VALUES (1,'Pustakawan','Pengurus Perpustakaan'),(2,'Administrasi','Pengurus Keuangan Perpustakaan');
UNLOCK TABLES;

CREATE TABLE IF NOT EXISTS `db_perpus`.`staff` (
	`stf_id` INT NOT NULL,
	`stf_u_id` INT NOT NULL,
	`stf_sc_id` INT NOT NULL,
	`stf_fullname` VARCHAR(80) NOT NULL,
	`stf_email` VARCHAR(45) NOT NULL,
	`stf_contact` VARCHAR(20) NOT NULL,
	`stf_address` VARCHAR(150) NOT NULL,
	`stf_shift` DATETIME NOT NULL,
	`stf_status` SMALLINT NOT NULL,
	`stf_rec_status` SMALLINT NOT NULL,
	`stf_rec_createdby` VARCHAR(45) NOT NULL,
	`stf_rec_created` DATETIME NOT NULL,
	`stf_rec_updatedby` VARCHAR(45) DEFAULT NULL,
	`stf_rec_updated` DATETIME NULL,
	`stf_rec_deletedby` VARCHAR(45) DEFAULT NULL,
	`stf_rec_deleted` DATETIME NULL,
	PRIMARY KEY (`stf_id`,`stf_sc_id`),
	UNIQUE KEY `stf_id_UNIQUE` (`stf_id`),
	CONSTRAINT `fk_users_staff` FOREIGN KEY (`stf_u_id`)
	REFERENCES `db_perpus`.`users` (`u_id`),
	CONSTRAINT `fk_staffCategory_staff` FOREIGN KEY (`stf_sc_id`)
	REFERENCES `db_perpus`.`staff_category` (`sc_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`book_rack` (
	`br_id` INT NOT NULL,
	`br_code` VARCHAR(16) NOT NULL,
	`br_room` VARCHAR(45) NOT NULL,
	`br_desc` VARCHAR(100) DEFAULT NULL,
	PRIMARY KEY (`br_id`),
	UNIQUE KEY `br_id_UNIQUE` (`br_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`book_category` (
	`bc_id` INT NOT NULL,
	`bc_br_id` INT NOT NULL,
	`bc_name` VARCHAR(45) NOT NULL,
	`bc_desc` VARCHAR(75) DEFAULT NULL,
	PRIMARY KEY (`bc_id`),
	UNIQUE KEY `bc_id_UNIQUE` (`bc_id`),
	CONSTRAINT `fk_bookRack_bookCategory` FOREIGN KEY (`bc_br_id`)
	REFERENCES `db_perpus`.`book_rack` (`br_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`member` (
	`m_id` INT NOT NULL,
	`m_u_id` INT NOT NULL,
	`m_class` VARCHAR(45) NOT NULL,
	`m_fullname` VARCHAR(80) NOT NULL,
	`m_email` VARCHAR(45) NOT NULL,
	`m_contact` VARCHAR(20) NOT NULL,
	`m_address` VARCHAR(150) NOT NULL,
	`m_status` SMALLINT NOT NULL,
	`m_rec_status` SMALLINT NOT NULL,
	`m_rec_createdby` VARCHAR(45) NOT NULL,
	`m_rec_created` DATETIME NOT NULL,
	`m_rec_updatedby` VARCHAR(45) DEFAULT NULL,
	`m_rec_updated` DATETIME NULL,
	`m_rec_deletedby` VARCHAR(45) DEFAULT NULL,
	`m_rec_deleted` DATETIME NULL,
	PRIMARY KEY (`m_id`),
	UNIQUE KEY `m_id_UNIQUE` (`m_id`),
	CONSTRAINT `fk_users_member` FOREIGN KEY (`m_u_id`)
	REFERENCES `db_perpus`.`users` (`u_id`)
)
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `db_perpus`.`books` (
	`bks_id` INT NOT NULL,
	`bks_bc_id` INT NOT NULL,
	`bks_m_id` INT DEFAULT NULL,
	`bks_code` VARCHAR(12) NOT NULL,
	`bks_name` VARCHAR(75) NOT NULL,
	`bks_writer` VARCHAR(45) NOT NULL,
	`bks_launcher` VARCHAR(45) NOT NULL,
	`bks_launchingtime` DATE NOT NULL,
	`bks_pages` INT NOT NULL,
	`bks_price` INT NOT NULL,
	`bks_rec_status` SMALLINT NOT NULL,
	`bks_rec_createdby` VARCHAR(45) NOT NULL,
	`bks_rec_created` DATETIME NOT NULL,
	`bks_rec_updatedby` VARCHAR(45) DEFAULT NULL,
	`bks_rec_updated` DATETIME NULL,
	`bks_rec_deletedby` VARCHAR(45) DEFAULT NULL,
	`bks_rec_deleted` DATETIME NULL,
	PRIMARY KEY (`bks_id`,`bks_bc_id`),
	UNIQUE KEY `bks_id_UNIQUE` (`bks_id`),
	CONSTRAINT `fk_bookCategory_books` FOREIGN KEY (`bks_bc_id`)
	REFERENCES `db_perpus`.`book_category` (`bc_id`),
	CONSTRAINT `fk_member_books` FOREIGN KEY (`bks_m_id`)
	REFERENCES `db_perpus`.`member` (`m_id`)
)
ENGINE = InnoDB;

-- End Setup MySQL Engineering
SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;