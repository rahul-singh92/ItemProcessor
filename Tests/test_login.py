import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

BASE_URL = "http://localhost:5000"


def wait_for(driver, by, value, timeout=10):
    return WebDriverWait(driver, timeout).until(
        EC.presence_of_element_located((by, value))
    )


class TestLogin:

    def test_tc01_valid_login(self, driver):
        """TC-01: Valid credentials redirect to Items page."""
        driver.get(f"{BASE_URL}/account/login")
        wait_for(driver, By.ID, "Email").send_keys("rahulsinghjadoun09@gmail.com")
        driver.find_element(By.ID, "Password").send_keys("1234567890")
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

        # Wait until we leave login page
        WebDriverWait(driver, 15).until(
            lambda d: "account" not in d.current_url.lower() and
                    d.current_url != f"{BASE_URL}/"
        )
        # Should be on item page
        assert "item" in driver.current_url.lower()
        driver.get(f"{BASE_URL}/account/logout")

    def test_tc02_wrong_password(self, driver):
        """TC-02: Wrong password stays on login — does not redirect."""
        driver.get(f"{BASE_URL}/account/login")
        wait_for(driver, By.ID, "Email").send_keys("admin@test.com")
        driver.find_element(By.ID, "Password").send_keys("WrongPassword999!")
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

        WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.ID, "Email"))
        )
        # Key check — must NOT have gone to item page
        assert "item" not in driver.current_url.lower()

    def test_tc03_nonexistent_email(self, driver):
        """TC-03: Non-existent email stays on login."""
        driver.get(f"{BASE_URL}/account/login")
        wait_for(driver, By.ID, "Email").send_keys("ghost@nowhere.com")
        driver.find_element(By.ID, "Password").send_keys("anypassword123")
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

        WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.ID, "Email"))
        )
        assert "item" not in driver.current_url.lower()
        assert "not found" not in driver.page_source.lower()

    def test_tc04_empty_fields(self, driver):
        """TC-04: Empty form stays on login page."""
        driver.get(f"{BASE_URL}/account/login")
        wait_for(driver, By.CSS_SELECTOR, "button[type='submit']").click()
        WebDriverWait(driver, 5).until(
            EC.presence_of_element_located((By.ID, "Email"))
        )
        assert "item" not in driver.current_url.lower()

    def test_tc05_invalid_email_format(self, driver):
        """TC-05: Invalid email format stays on login."""
        driver.get(f"{BASE_URL}/account/login")
        wait_for(driver, By.ID, "Email").send_keys("notanemail")
        driver.find_element(By.ID, "Password").send_keys("pass123456")
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
        WebDriverWait(driver, 5).until(
            EC.presence_of_element_located((By.ID, "Email"))
        )
        assert "item" not in driver.current_url.lower()