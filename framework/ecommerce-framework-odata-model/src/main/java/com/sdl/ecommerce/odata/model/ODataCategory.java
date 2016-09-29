package com.sdl.ecommerce.odata.model;

import com.sdl.ecommerce.api.model.Category;
import com.sdl.odata.api.edm.annotations.EdmEntity;
import com.sdl.odata.api.edm.annotations.EdmEntitySet;
import com.sdl.odata.api.edm.annotations.EdmNavigationProperty;
import com.sdl.odata.api.edm.annotations.EdmProperty;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

/**
 * OData Category
 *
 * @author nic
 */
@EdmEntity(name="Category", key = "id", namespace = "SDL.ECommerce")
@EdmEntitySet(name="Categories")
public class ODataCategory implements Category {

    @EdmProperty(name = "id", nullable = false)
    protected String id;

    @EdmProperty(name = "name", nullable = false)
    protected String name;

    // Property only used for query based on path
    //
    @EdmProperty(name = "path")
    private String path;

    @EdmProperty(name = "pathName")
    private String pathName;

    @EdmProperty(name = "locale")
    private String locale;  //????

    @EdmNavigationProperty(name = "parent")
    private ODataCategory parent = null;

    @EdmNavigationProperty(name = "categories")
    private List<ODataCategory> categories = null;

    @EdmProperty(name= "parentIds")
    private List<String> parentIds = null;

    private NavigationPropertyResolver navigationPropertyResolver = null;

    public ODataCategory() {}

    public ODataCategory(Category category) {
        this.id = category.getId();
        this.name = category.getName();
        this.pathName = category.getPathName();

        this.parentIds = new ArrayList<>();
        Category parent = category.getParent();
        while ( parent != null ) {
            this.parentIds.add(0, parent.getId());
            parent = parent.getParent();
        }
    }

    public void setNavigationPropertyResolver(NavigationPropertyResolver navigationPropertyResolver) {
        this.navigationPropertyResolver = navigationPropertyResolver;
    }

    @Override
    public List<Category> getCategories() {
        if ( this.categories == null && this.navigationPropertyResolver != null ) {
            this.categories = (List<ODataCategory>) this.navigationPropertyResolver.resolve(this.id, "categories");
        }
        return this.categories.stream().collect(Collectors.toList());
    }

    @Override
    public Category getParent() {
        // TODO: What happens if category does not have a parent???
        if ( this.parent == null && this.navigationPropertyResolver != null ) {
            this.parent = (ODataCategory) this.navigationPropertyResolver.resolve(this.id, "parent");
        }
        return this.parent;
    }

    @Override
    public String getId() {
        return this.id;
    }

    @Override
    public String getName() {
        return this.name;
    }


    @Override
    public String getPathName() {
        return this.pathName;
    }
}
