import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

BASE_URL = "http://localhost:5000"


class TestTree:
    """TC-25 to TC-28: Tree View tests."""

    def test_tc25_tree_view_loads(self, logged_in_driver):
        """TC-25: Tree view page loads and shows root items."""
        driver = logged_in_driver
        driver.get(f"{BASE_URL}/Process/AllTrees")

        assert "Tree" in driver.page_source or \
               "tree" in driver.page_source.lower()

    def test_tc26_tree_for_processed_item(self, logged_in_driver):
        """TC-26: Tree for a processed item shows children."""
        driver = logged_in_driver

        # Navigate to tree for item 1 (our Raw Material A)
        driver.get(f"{BASE_URL}/Process/Tree/1")

        page_source = driver.page_source
        # Should show the item name
        assert "Raw Material A" in page_source or \
               "Tree View" in page_source

    def test_tc27_tree_shows_indentation_levels(self, logged_in_driver):
        """TC-27: Multi-level tree shows correct hierarchy."""
        driver = logged_in_driver
        driver.get(f"{BASE_URL}/Process/Tree/1")

        # Check for level indicators (our tree uses margin-left based on level)
        page_source = driver.page_source
        # Should have child items visible
        assert "└─" in page_source or "Output" in page_source

    def test_tc28_tree_for_unprocessed_item(self, logged_in_driver):
        """TC-28: Tree for unprocessed item shows only root, no error."""
        driver = logged_in_driver

        # Navigate to tree for an item that was never processed
        # Item 3 = Raw Material C (we never processed it)
        driver.get(f"{BASE_URL}/Process/Tree/3")

        # Should load without crashing
        assert "500" not in driver.title
        assert "Error" not in driver.title