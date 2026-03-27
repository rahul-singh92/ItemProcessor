import pytest
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

BASE_URL = "http://localhost:5000"


@pytest.fixture(scope="session")
def driver():
    """
    One browser for the entire test session.
    Why session scope? Opening/closing Chrome per test wastes 3-5 seconds each.
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


@pytest.fixture(scope="session", autouse=True)
def login_once(driver):
    driver.get(f"{BASE_URL}/Account/Login")

    WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.ID, "Email"))
    )

    driver.find_element(By.ID, "Email").send_keys("rahulsinghjadoun09@gmail.com")
    driver.find_element(By.ID, "Password").send_keys("1234567890")
    driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

    # Wait until we leave Account pages entirely
    WebDriverWait(driver, 15).until(
        lambda d: "Account" not in d.current_url and
                  "Login" not in d.current_url
    )


@pytest.fixture
def logged_in_driver(driver):
    """
    Simple fixture — just returns the already-logged-in driver.
    No login/logout per test — session stays alive the whole time.
    """
    yield driver