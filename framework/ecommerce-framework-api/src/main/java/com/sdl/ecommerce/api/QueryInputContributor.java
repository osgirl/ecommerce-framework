package com.sdl.ecommerce.api;

/**
 * Query Input Contributor
 * - Is used to go through all widgets on the page to gather all input from them to the product query
 *
 * @author nic
 */
public interface QueryInputContributor {

    // TODO: Rename to QueryContributor

    /**
     * Contribute to the query. Here the widget component can attach parameters (set by metadata fields etc)
     * to influence the query, such as number of items in lister, show all possible facets or not, specific category etc
     *
     * @param query
     */
    void contributeToQuery(Query query);
}
