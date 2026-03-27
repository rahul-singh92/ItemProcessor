import pytest
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

BASE_URL = "http://localhost:5000"


@pytest.fixture(scope="module")
def fresh_driver():
    """
    Separate browser just for logout tests.
    Why module scope? These tests need their own clean session
    that doesn't affect the main session-scoped driver.
    """
    options = Options()
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument("--window-size=1280,800")

    d = webdriver.Chrome(
        service=Service(ChromeDriverManager().install()),
        options=options
    )
    d.implicitly_wait(0)
    yield d
    d.quit()


def wait_for(driver, by, value, timeout=10):
    return WebDriverWait(driver, timeout).until(
        EC.presence_of_element_located((by, value))
    )


def do_login(driver):
    driver.get(f"{BASE_URL}/Account/Login")
    wait_for(driver, By.ID, "Email").send_keys("admin@test.com")
    driver.find_element(By.ID, "Password").send_keys("Admin@123")
    driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

    # Wait until we leave the Account/Login page completely
    WebDriverWait(driver, 15).until(
        lambda d: "Account" not in d.current_url and
                  "Login" not in d.current_url
    )


class TestAuthSpecial:

    def test_tc06_protected_route_redirect(self, fresh_driver):
        """TC-06: /Item/Index without login must redirect to Login."""
        fresh_driver.get(f"{BASE_URL}/Account/Logout")
        WebDriverWait(fresh_driver, 10).until(
            lambda d: "Login" in d.page_source or
                    "Account" in d.current_url
        )

        # Use the actual working URL
        fresh_driver.get(f"{BASE_URL}/Item/Index")

        WebDriverWait(fresh_driver, 10).until(
            lambda d: "Account" in d.current_url or
                    "Login" in d.page_source
        )
        assert "Login" in fresh_driver.page_source or \
            "Account" in fresh_driver.current_url

    def test_tc07_logout_redirect(self, fresh_driver):
        """TC-07: After logout /Item/Index redirects to Login."""
        do_login(fresh_driver)
        assert "Login" not in fresh_driver.current_url

        fresh_driver.get(f"{BASE_URL}/Account/Logout")
        WebDriverWait(fresh_driver, 10).until(
            lambda d: "Login" in d.page_source or
                    "Account" in d.current_url
        )

        # Use actual working URL
        fresh_driver.get(f"{BASE_URL}/Item/Index")

        WebDriverWait(fresh_driver, 10).until(
            lambda d: "Account" in d.current_url or
                    "Login" in d.page_source
        )
        assert "Login" in fresh_driver.page_source or \
            "Account" in fresh_driver.current_url