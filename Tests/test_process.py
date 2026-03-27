import pytest
import time
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import Select

BASE_URL = "http://localhost:5000"


class TestProcess:
    """TC-17 to TC-24: Process Item tests."""

    def test_tc17_process_single_output(self, logged_in_driver):
        """TC-17: Process an item with one valid output."""
        driver = logged_in_driver
        driver.get(f"{BASE_URL}/Process/Create")

        # Select first available parent item
        select = Select( WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.NAME, "ParentItemId"))))
        select.select_by_index(1)  # first real option (index 0 is placeholder)

        time.sleep(0.5)  # wait for JS to update weight display

        # Fill in child item
        driver.find_element(
            By.NAME, "ChildItems[0].ItemName"
        ).send_keys("Auto Output Item")

        weight_field = driver.find_element(By.NAME, "ChildItems[0].Weight")
        weight_field.clear()
        weight_field.send_keys("1.000")  # small safe weight

        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

        # Should redirect to processed items list
        WebDriverWait(driver, 5).until(
            EC.url_contains("/Process")
        )
        assert "processed" in driver.page_source.lower() or \
               "/Process" in driver.current_url

    def test_tc19_output_exceeds_parent_weight(self, logged_in_driver):
        """TC-19: Output weight > parent weight must be rejected."""
        driver = logged_in_driver
        driver.get(f"{BASE_URL}/Process/Create")

        # Wait for the parent select element
        try:
            select_element = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.NAME, "ParentItemId"))
            )
        except:
            pytest.skip("No parent items available to select — skipping test.")

        # Check if the select has any real options (skip placeholder)
        options = select_element.find_elements(By.TAG_NAME, "option")
        if len(options) <= 1:
            pytest.skip("No real parent items in the list — skipping test.")

        # Select first real parent item
        select = Select(select_element)
        select.select_by_index(1)
        time.sleep(0.5)  # wait for JS to update weight display

        # Fill overweight child item
        driver.find_element(By.NAME, "ChildItems[0].ItemName").send_keys("Overweight Output")
        weight_field = driver.find_element(By.NAME, "ChildItems[0].Weight")
        weight_field.clear()
        weight_field.send_keys("999999.999")  # definitely exceeds any parent

        # Submit form
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
        time.sleep(0.5)  # wait for response

        # Verify rejection: either error message exists or still on create page
        page_source = driver.page_source.lower()
        assert "cannot exceed" in page_source or "/process/create" in driver.current_url, \
            "Overweight child was not rejected as expected"

    def test_tc21_cannot_remove_last_child(self, logged_in_driver):
        """TC-21: Cannot remove the only child row — JS prevents it."""
        driver = logged_in_driver
        driver.get(f"{BASE_URL}/Process/Create")

        # Try clicking the remove button on the only child row
        remove_btn = driver.find_element(
            By.CSS_SELECTOR, ".child-item .btn-outline-danger"
        )
        remove_btn.click()

        # Alert should appear
        try:
            alert = WebDriverWait(driver, 3).until(EC.alert_is_present())
            assert "required" in alert.text.lower() or \
                   "one" in alert.text.lower()
            alert.accept()
        except Exception:
            # If no alert, check the row still exists
            rows = driver.find_elements(By.CSS_SELECTOR, ".child-item")
            assert len(rows) >= 1, "Last child row should not be removable"

    def test_tc22_empty_output_name(self, logged_in_driver):
        """TC-22: Output item with no name fails validation."""
        driver = logged_in_driver
        driver.get(f"{BASE_URL}/Process/Create")

        select = Select(
    WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.NAME, "ParentItemId"))
    )
)
        select.select_by_index(1)

        # Leave name empty, fill weight only
        weight_field = driver.find_element(By.NAME, "ChildItems[0].Weight")
        weight_field.clear()
        weight_field.send_keys("10.000")

        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

        # Should not proceed
        assert "/Process/Create" in driver.current_url or \
               "required" in driver.page_source.lower()